# EFT DMA Quest Manager — Implementation Reference

> **Audience:** LLM / developer implementing a Quest Helper in an EFT DMA radar from scratch.
> **Game version context:** IL2CPP (post-Unity 2021 migration). All offsets below are for the March 2026 patch unless noted. Mono-era offsets are commented where they differ.

---

## Table of Contents
1. [Architecture Overview](#1-architecture-overview)
2. [Memory Structures and Offsets](#2-memory-structures-and-offsets)
3. [Reading Quests from Memory](#3-reading-quests-from-memory)
4. [HashSet\<MongoID\> Parsing](#4-hashsetmongoid-parsing)
5. [Static Zone Data (EFT API)](#5-static-zone-data-eft-api)
6. [Data Model Classes](#6-data-model-classes)
7. [Refresh Loop and Rate Limiting](#7-refresh-loop-and-rate-limiting)
8. [Map Filtering](#8-map-filtering)
9. [ESP Rendering — Quest Info Widget](#9-esp-rendering--quest-info-widget)
10. [ESP Rendering — World Quest Zones](#10-esp-rendering--world-quest-zones)
11. [Configuration Flags](#11-configuration-flags)
12. [Common Pitfalls](#12-common-pitfalls)
13. [EQuestStatus Enum Reference](#13-equeststatus-enum-reference)

---

## 1. Architecture Overview

The system is split into three layers:

```
┌─────────────────────────────────────────────────────────────────┐
│  MEMORY LAYER                                                   │
│  QuestManager.Refresh()                                         │
│  · Reads Profile.QuestsData  → List<QuestStatusData>           │
│  · Per quest: reads Status, ID, CompletedConditions            │
│  · CompletedConditions is a HashSet<MongoID> in IL2CPP         │
└───────────────────────────┬─────────────────────────────────────┘
                            │ quest IDs + completed condition IDs
┌───────────────────────────▼─────────────────────────────────────┐
│  STATIC DATA LAYER (tarkov.dev API / bundled JSON)             │
│  EftDataManager.TaskData  Dictionary<questId, TaskElement>      │
│  · Quest names, Kappa flag, objective descriptions              │
│  · Zone positions (Vector3) and outline polygons               │
│  · Required item/key IDs per objective                         │
└───────────────────────────┬─────────────────────────────────────┘
                            │ merged Quest objects
┌───────────────────────────▼─────────────────────────────────────┐
│  PRESENTATION LAYER                                             │
│  ESPQuestInfoWidget / QuestInfoWidget  — HUD overlay           │
│  QuestLocation (IMapEntity / IESPEntity) — world markers       │
└─────────────────────────────────────────────────────────────────┘
```

**Critical design decision:** Objective descriptions, zone positions, and item names are **never parsed from game memory**. They come exclusively from the static API dataset. Memory provides only:
- Which quests are `Started` (status == 2)
- Which condition IDs have been completed (the `HashSet<MongoID>`)

This avoids parsing fragile IL2CPP `IEnumerable<Condition>` LINQ iterators, which change layout every patch.

---

## 2. Memory Structures and Offsets

### 2.1 Reaching the Quest List

```
LocalPlayer
  └─ Profile                          Profile.QuestsData = 0x98
       └─ List<QuestStatusData>       (System.Collections.Generic.List<T>)
            ├─ _items  (ulong[])      UnityOffsets.ManagedList.ItemsPtr  (typically 0x10)
            └─ _size   (int)          UnityOffsets.ManagedList.Count     (typically 0x18)
```

The list itself uses the standard IL2CPP managed list layout:
- `+0x10` → pointer to the backing array object (`T[]`)
- `+0x18` → `int` count of valid elements

The backing array object starts with the standard managed array header; elements begin at:
- `UnityOffsets.ManagedArray.FirstElement` (typically `0x20`)
- Each element is a pointer (`ulong`), stride = `UnityOffsets.ManagedArray.ElementSize` (typically `0x8`)

### 2.2 QuestStatusData (IL2CPP)

```
Offset  Type        Field
0x10    String*     Id           — MongoDB ObjectId string (24 hex chars)
0x1C    int         Status       — EQuestStatus enum value
0x28    HashSet*    CompletedConditions — HashSet<MongoID> (see section 4)
0x38    QuestTemplate* Template  — optional, rarely needed at runtime
```

> **Mono era (pre-IL2CPP):** `Status` was at `0x34`, `CompletedConditions` at `0x20`, `Template` at `0x28`.

### 2.3 QuestTemplate (rarely needed)

```
Offset  Type        Field
0x48    String*     TraderId
0x60    Object*     Conditions   → ConditionsDict
0xC8    String*     Name         — quest display name (use API instead)
0x118   int         EQuestType
0x120   bool        IsMainQuest
```

`Conditions` at `0x60` points to a `ConditionsDict` which has a `_necessaryConditions` list at `+0x70`. Parsing this via DMA is unreliable (LINQ state machines) — use the API dataset for objective data instead.

### 2.4 Profile offsets (parent struct)

```
Offset  Field
0x18    AccountId   (String)
0x48    Info        (ProfileInfo)
0x70    Inventory
0x98    QuestsData  (List<QuestStatusData>)  ← the one we need
0x108   WishlistManager
```

### 2.5 UnityOffsets referenced

```csharp
// Standard managed list
ManagedList.ItemsPtr  = 0x10  // pointer to T[] backing array
ManagedList.Count     = 0x18  // int element count

// Standard managed array
ManagedArray.FirstElement = 0x20  // first T in T[]
ManagedArray.ElementSize  = 0x08  // pointer stride

// HashSet<T> internal entries array
IL2CPPHashSet2.Entries         = 0x18  // pointer to Entry[] array object
IL2CPPHashSet2.Count           = 0x1C  // int _count
IL2CPPHashSet2.EntrySize       = 0x18  // sizeof(Entry<MongoID>)
IL2CPPHashSet2.EntryValueOffset = 0x08 // offset of value within entry (after hashCode+next)

// MongoID struct layout
MongoID.StringID = 0x10  // string* _stringID within the struct
```

---

## 3. Reading Quests from Memory

### 3.1 Entry point

```csharp
// _profile is the address of the local player's Profile object
ulong questsData = Memory.ReadValue<ulong>(_profile + Offsets.Profile.QuestsData);
if (questsData == 0 || !IsValidVA(questsData)) return;

ulong listItemsPtr = Memory.ReadValue<ulong>(questsData + ManagedList.ItemsPtr);
int   listCount    = Memory.ReadValue<int>  (questsData + ManagedList.Count);

if (listCount <= 0 || listCount > 500) return;  // sanity bounds
```

### 3.2 Iterating quests

```csharp
for (int i = 0; i < listCount; i++)
{
    ulong qDataEntry = Memory.ReadValue<ulong>(
        listItemsPtr + ManagedArray.FirstElement + (ulong)(i * ManagedArray.ElementSize));

    if (qDataEntry == 0) continue;

    // Filter: only process Started quests
    int status = Memory.ReadValue<int>(qDataEntry + Offsets.QuestData.Status);
    if (status != 2) continue;  // 2 = EQuestStatus.Started

    // Read quest ID string
    ulong qIdPtr = Memory.ReadPtr(qDataEntry + Offsets.QuestData.Id);
    string questId = Memory.ReadUnityString(qIdPtr);  // reads UTF-16 Unity string
    if (string.IsNullOrEmpty(questId)) continue;

    // Read completed conditions
    ulong completedHashSetPtr = Memory.ReadValue<ulong>(
        qDataEntry + Offsets.QuestData.CompletedConditions);
    var completedConditions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    if (completedHashSetPtr != 0)
        ReadHashSetMongoIds(completedHashSetPtr, completedConditions);

    // Build Quest object from static API data + memory-sourced conditions
    var quest = BuildQuest(questId, completedConditions);
    if (quest != null)
        activeQuests.Add(quest);
}
```

### 3.3 Building a Quest from API data

```csharp
Quest BuildQuest(string questId, HashSet<string> completedConditions)
{
    // All quest metadata comes from the static dataset, not memory
    if (!EftDataManager.TaskData.TryGetValue(questId, out var taskData))
        return null;  // quest not in API (story quest or new content)

    var quest = new Quest
    {
        Id                  = questId,
        Name                = taskData.Name ?? "Unknown Quest",
        KappaRequired       = taskData.KappaRequired,
        CompletedConditions = completedConditions,
        Objectives          = new List<QuestObjective>()
    };

    foreach (var apiObj in taskData.Objectives ?? Enumerable.Empty<ObjectiveElement>())
    {
        bool isCompleted = !string.IsNullOrEmpty(apiObj.Id)
                        && completedConditions.Contains(apiObj.Id);

        var objective = new QuestObjective
        {
            Id               = apiObj.Id ?? "",
            Type             = MapObjectiveType(apiObj.Type),
            Optional         = apiObj.Optional,
            Description      = apiObj.Description ?? "",
            IsCompleted      = isCompleted,
            RequiredItemIds  = ExtractItemIds(apiObj),
            LocationObjectives = BuildLocationMarkers(questId, apiObj)
        };

        quest.Objectives.Add(objective);
    }

    // Collect all required item IDs from incomplete objectives
    quest.RequiredItems = quest.Objectives
        .Where(o => !o.IsCompleted)
        .SelectMany(o => o.RequiredItemIds)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    return quest;
}
```

---

## 4. HashSet\<MongoID\> Parsing

This is the most complex part. `MongoID` is a **value type struct** stored inline inside each `HashSet.Entry<T>`.

### 4.1 IL2CPP HashSet\<MongoID\> memory layout

```
HashSet<MongoID> object
  +0x18  → Entry[] entries  (pointer to managed array object)
  +0x1C  → int _count       (number of valid entries; try 0x20 and 0x3C as fallbacks)

Entry[] object (standard managed array)
  +0x20  → Entry[0]
  +0x38  → Entry[1]
  ...     stride = 0x18 per entry (IL2CPPHashSet2.EntrySize)

Entry<MongoID> layout (inline struct, 0x18 bytes):
  +0x00  int  hashCode    (-1 for free/deleted slots)
  +0x04  int  next        (next entry index in collision chain, -1 = end)
  +0x08  MongoID value    (inline value type — MongoID struct)
         ├── +0x00  uint   _timeStamp
         ├── +0x08  ulong  _counter
         └── +0x10  String* _stringID  ← this is what we need

Reading a condition ID:
  entryBase = entries_array_object + 0x20 + (i * 0x18)
  stringIdPtr = ReadPtr(entryBase + 0x08 + 0x10)   // EntryValueOffset + MongoID.StringID
  conditionId = ReadUnityString(stringIdPtr)
```

### 4.2 Implementation

```csharp
void ReadHashSetMongoIds(ulong hashSetPtr, HashSet<string> output)
{
    if (hashSetPtr == 0) return;

    ulong entriesPtr = Memory.ReadPtr(hashSetPtr + 0x18);  // IL2CPPHashSet2.Entries

    // Try multiple count offsets — varies by IL2CPP version
    int count = Memory.ReadValue<int>(hashSetPtr + 0x1C);
    if (count <= 0 || count > 100)
        count = Memory.ReadValue<int>(hashSetPtr + 0x20);
    if (count <= 0 || count > 100)
        count = Memory.ReadValue<int>(hashSetPtr + 0x3C);

    if (entriesPtr == 0 || count <= 0 || count > 100) return;

    const ulong FIRST_ELEMENT    = 0x20;  // ManagedArray.FirstElement
    const ulong ENTRY_SIZE       = 0x18;  // IL2CPPHashSet2.EntrySize
    const ulong ENTRY_VAL_OFFSET = 0x08;  // IL2CPPHashSet2.EntryValueOffset
    const ulong MONGOID_STRINGID = 0x10;  // MongoID.StringID

    for (int i = 0; i < count && output.Count < 50; i++)
    {
        try
        {
            ulong entryBase    = entriesPtr + FIRST_ELEMENT + (ulong)(i * (long)ENTRY_SIZE);
            ulong stringIdPtr  = Memory.ReadPtr(entryBase + ENTRY_VAL_OFFSET + MONGOID_STRINGID);

            if (stringIdPtr == 0 || stringIdPtr < 0x10000000) continue;  // null or kernel address

            string conditionId = Memory.ReadUnityString(stringIdPtr);

            // Sanity check: condition IDs are MongoDB ObjectIds or similar (~24-64 chars)
            if (!string.IsNullOrEmpty(conditionId)
             && conditionId.Length > 10
             && conditionId.Length < 100)
            {
                output.Add(conditionId);
            }
        }
        catch { /* skip corrupt entry */ }
    }
}
```

### 4.3 Why only `CompletedConditions` and not `LocalChanges`

The `QuestStatusData.CompletedConditions` field in IL2CPP **is directly a `HashSet<MongoID>`** (not the `CompletedConditionsCollection` wrapper that older Mono builds used). The `CompletedConditionsCollection` struct (with `BackendData` at `+0x10` and `LocalChanges` at `+0x18`) was a Mono-era wrapper that is obsolete in IL2CPP. Read the single `HashSet` directly at offset `0x28`.

---

## 5. Static Zone Data (EFT API)

Quest zone positions, outlines, and objective metadata are fetched from the [tarkov.dev GraphQL API](https://tarkov.dev/api/) at startup and cached as `EftDataManager.TaskData`.

### 5.1 Data shape (abbreviated)

```csharp
class TaskElement
{
    string Id;
    string Name;
    bool   KappaRequired;
    List<ObjectiveElement> Objectives;
}

class ObjectiveElement
{
    string Id;            // condition ID used to match CompletedConditions
    string Type;          // "find", "giveItem", "kill", "visit", "mark", "plantItem", etc.
    string Description;   // human-readable text
    bool   Optional;
    ItemRef  Item;        // { Id, Name, ShortName }
    ItemRef  QuestItem;   // quest-specific items
    ItemRef  MarkerItem;  // items to place
    List<KeyGroup> RequiredKeys;  // [[key1, key2], [key3]] = (key1 OR key2) AND key3
    List<MapRef> Maps;    // maps this objective is relevant on
    List<ZoneElement> Zones;
}

class ZoneElement
{
    string Id;          // zone identifier used to index _questZones
    MapRef Map;         // { Id: BSG map MongoID }
    Position Position;  // { X, Y, Z }
    List<Position> Outline;  // polygon boundary points (optional)
}
```

### 5.2 Building the zone cache

Zone positions are compiled once at startup (and re-built when config filters change) into two frozen dictionaries:

```csharp
// _questZones[mapBsgId][zoneId] = Vector3 center position
FrozenDictionary<string, FrozenDictionary<string, Vector3>> _questZones;

// _questOutlines[mapBsgId][zoneId] = List<Vector3> polygon vertices
FrozenDictionary<string, FrozenDictionary<string, List<Vector3>>> _questOutlines;
```

Build process:
1. Iterate all `TaskData` objectives
2. Filter by `KappaFilter` and `OptionalTaskFilter` config flags
3. Flatten all `Zones` from matching objectives
4. Group by `zone.Map.Id` (BSG MongoID of the map)
5. De-duplicate zone IDs within each map

### 5.3 Map ID translation table

EFT uses internal map names (e.g., `"bigmap"`) at the memory level. The tarkov.dev API uses BSG MongoIDs. The bridge:

```csharp
// game MapID string → BSG MongoID used in tarkov.dev zone data
var mapToId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    { "factory4_day",   "55f2d3fd4bdc2d5f408b4567" },
    { "factory4_night", "59fc81d786f774390775787e" },
    { "bigmap",         "56f40101d2720b2a4d8b45d6" },  // Customs
    { "woods",          "5704e3c2d2720bac5b8b4567" },
    { "lighthouse",     "5704e4dad2720bb55b8b4567" },
    { "shoreline",      "5704e554d2720bac5b8b456e" },
    { "labyrinth",      "6733700029c367a3d40b02af" },
    { "rezervbase",     "5704e5fad2720bc05b8b4567" },  // Reserve
    { "interchange",    "5714dbc024597771384a510d" },
    { "tarkovstreets",  "5714dc692459777137212e12" },
    { "laboratory",     "5b0fc42d86f7744a585f9105" },
    { "Sandbox",        "653e6760052c01c1c805532f" },   // Ground Zero
    { "Sandbox_high",   "65b8d6f5cdde2479cb2a3125" },
};
```

---

## 6. Data Model Classes

```csharp
public sealed class Quest
{
    public string Id { get; set; }
    public string Name { get; set; }
    public bool KappaRequired { get; set; }
    public List<QuestObjective> Objectives { get; set; }
    public HashSet<string> RequiredItems { get; set; }        // item BSG IDs needed in current raid
    public HashSet<string> CompletedConditions { get; set; }  // condition IDs done (from memory)

    public bool IsCompleted => Objectives.All(o => o.IsCompleted);
    public int CompletedObjectivesCount => Objectives.Count(o => o.IsCompleted);
    public int TotalObjectivesCount => Objectives.Count;
}

public sealed class QuestObjective
{
    public string Id { get; set; }                       // condition ID for matching CompletedConditions
    public QuestObjectiveType Type { get; set; }
    public bool Optional { get; set; }
    public string Description { get; set; }
    public bool IsCompleted { get; set; }
    public List<string> RequiredItemIds { get; set; }    // BSG item IDs
    public List<QuestLocation> LocationObjectives { get; set; }  // map markers

    public bool HasLocationRequirement => LocationObjectives.Any();
    public bool HasItemRequirement => RequiredItemIds.Any();
}

public enum QuestObjectiveType
{
    FindItem,       // "find", "giveItem"
    PlaceItem,      // "mark", "plantItem"
    VisitLocation,  // "visit"
    LaunchFlare,
    ZoneObjective,
    InZone,
    Other           // "kill", "shoot", "extract"
}

// Objective type mapping from API strings:
// "find" | "giveitem"   → FindItem
// "mark" | "plantitem"  → PlaceItem
// "visit"               → VisitLocation
// everything else       → Other
```

### 6.1 QuestLocation (world entity)

`QuestLocation` implements `IMapEntity`, `IESPEntity`, `IWorldEntity`, and `IMouseoverEntity`. It wraps a 3D position (and optional polygon outline) to be rendered on the map/ESP.

```csharp
public sealed class QuestLocation : IWorldEntity, IMapEntity, IMouseoverEntity, IESPEntity
{
    public string QuestID { get; }
    public string QuestName { get; }       // display name from API
    public string LocationName { get; }   // zone ID string
    public string ObjectiveId { get; }
    public string MapId { get; }           // BSG MongoID of current map
    public bool Optional { get; }
    public List<Vector3> Outline { get; } // null if no outline data
    // position comes from IWorldEntity.Position
}
```

A `QuestLocation` is created with or without an outline:
- **With outline** (`CreateQuestLocationWithOutline`): preferred; tries to find polygon from `_questOutlines`
- **Without outline** (`CreateQuestLocation`): fallback; only center point from `_questZones`

---

## 7. Refresh Loop and Rate Limiting

The `QuestManager` is instantiated once per raid and lives on the game loop thread.

```csharp
public class QuestManager
{
    private readonly Stopwatch _rateLimit = new();
    private readonly ulong _profile;  // address of LocalPlayer.Profile

    public QuestManager(ulong profile)
    {
        _profile = profile;
        Refresh();  // immediate first read
    }

    public void Refresh()
    {
        UpdateCaches();  // rebuild zone dictionaries if config changed

        // Rate limit: at most once every 2 seconds
        if (_rateLimit.IsRunning && _rateLimit.Elapsed.TotalSeconds < 2.0)
            return;

        // ... memory reads ...

        _rateLimit.Restart();
    }
}
```

**Important:** `Refresh()` is called from the game loop's `game.Refresh()` each frame (~133ms tick). The 2-second rate limit prevents hammering DMA reads for quest data that rarely changes.

The zone caches are rebuilt (via `UpdateCaches()`) whenever:
- `Config.QuestHelper.KappaFilter` changes
- `Config.QuestHelper.OptionalTaskFilter` changes
- On the first call (caches are null)
- If caches are empty but `EftDataManager.TaskData` is now populated (late API load)

---

## 8. Map Filtering

### 8.1 Which quests are shown for the current map

```csharp
public IEnumerable<Quest> GetQuestsForCurrentMap()
{
    if (!_mapToId.TryGetValue(Memory.MapID, out var currentMapBsgId))
        return Enumerable.Empty<Quest>();

    return ActiveQuests.Where(quest =>
    {
        // Check memory-parsed location objectives
        bool hasMemoryLocation = quest.Objectives.Any(obj =>
            obj.LocationObjectives.Any(loc => loc.MapId == currentMapBsgId)
            || obj.HasLocationRequirement);

        if (hasMemoryLocation) return true;

        // Check API data for map associations
        if (EftDataManager.TaskData.TryGetValue(quest.Id, out var taskData))
            return taskData.Objectives?.Any(o =>
                o.Maps?.Any(m => m.Id == currentMapBsgId) == true) == true;

        return false;
    });
}

public IEnumerable<Quest> GetOtherQuests()
{
    var currentMapQuestIds = GetQuestsForCurrentMap().Select(q => q.Id).ToHashSet();
    return ActiveQuests.Where(q => !currentMapQuestIds.Contains(q.Id));
}
```

### 8.2 Required items lookup

```csharp
// Queried from loot rendering code to highlight quest items
public bool IsItemRequired(string itemBsgId)
    => RequiredItems.Contains(itemBsgId);
```

This is populated from all incomplete objectives across all active quests, regardless of map. Loot items matching any `RequiredItems` entry can be highlighted.

### 8.3 Item IDs extraction from API objectives

```csharp
List<string> ExtractItemIds(ObjectiveElement apiObj)
{
    var ids = new List<string>();
    if (!string.IsNullOrEmpty(apiObj.Item?.Id))       ids.Add(apiObj.Item.Id);      // find/collect
    if (!string.IsNullOrEmpty(apiObj.QuestItem?.Id))  ids.Add(apiObj.QuestItem.Id); // quest item
    if (!string.IsNullOrEmpty(apiObj.MarkerItem?.Id)) ids.Add(apiObj.MarkerItem.Id); // place marker
    return ids;
}
```

---

## 9. ESP Rendering — Quest Info Widget

The `ESPQuestInfoWidget` (and its main-window twin `QuestInfoWidget`) renders a scrollable HUD panel. Both use SkiaSharp.

### 9.1 Paint objects (color coding)

```csharp
// Quest name header line
SKPaint _questNamePaint    = { Color = SKColors.LightBlue };

// Objective lines
SKPaint _questIncompletePaint = { Color = SKColors.White };
SKPaint _questCompletedPaint  = { Color = SKColors.Green };
SKPaint _questOptionalPaint   = { Color = SKColors.Gray };

// Sub-items
SKPaint _questKeyPaint  = { Color = SKColors.Yellow };
SKPaint _questItemPaint = { Color = SKColors.Orange };

// Strikethrough line for completed objectives
SKPaint _questStrikethroughPaint = { Color = SKColors.Green, Style = Stroke, StrokeWidth = 1.3f };
```

### 9.2 Line format

```
[-] Quest Name Here          ← collapsible header (LightBlue)
  ○ Find the Pocket Watch    ← incomplete objective (White)
  ✓ Place the letter         ← completed (Green + strikethrough)
  ○ [Optional] Extra task    ← optional incomplete (Gray)
    Key: Factory exit key    ← required key if "Show Keys" enabled (Yellow)
    Item: Pocket Watch       ← required item if "Show Items" enabled (Orange)
```

The `[-]` / `[+]` prefix is clickable to collapse/expand the quest.

### 9.3 Draw loop (simplified)

```csharp
void Draw(SKCanvas canvas)
{
    var questManager = Memory.QuestManager;
    if (questManager == null) return;

    var mapQuests = questManager.GetQuestsForCurrentMap();
    if (KappaFilter) mapQuests = mapQuests.Where(q => q.KappaRequired);

    var drawPt = new SKPoint(left + padding, top + lineSpacing + padding);

    // Header
    DrawLine(canvas, $"Active Quests on {mapName}:", ref drawPt, _questTextPaint);

    foreach (var quest in mapQuests)
    {
        var collapsed = _collapsedQuests.GetValueOrDefault(quest.Id, false);
        DrawLine(canvas, $"{(collapsed ? "[+]" : "[-]")} {quest.Name}", ref drawPt, _questNamePaint);

        if (collapsed) continue;

        // Prefer API objectives (have keys/items metadata); fall back to memory-parsed
        var objectives = EftDataManager.TaskData.TryGetValue(quest.Id, out var td) && td.Objectives != null
            ? (IEnumerable<...>)td.Objectives
            : quest.Objectives;

        foreach (var obj in objectives)
        {
            bool done = quest.CompletedConditions.Contains(obj.Id);
            if (_hideCompleted && done) continue;
            if (obj.Optional && !OptionalTaskFilter) continue;

            var symbol = done ? "✓" : "○";
            var optPrefix = obj.Optional ? "[Optional] " : "";
            var paint = done ? _questCompletedPaint
                       : obj.Optional ? _questOptionalPaint
                       : _questIncompletePaint;

            DrawTextWithStrikethrough(canvas, $"  {symbol} {optPrefix}{obj.Description}",
                                      drawPt, paint, strikethrough: done);
            drawPt.Y += lineSpacing;

            // Optional key/item sub-lines
            if (_showKeys && !done && obj.RequiredKeys != null)
                foreach (var key in obj.RequiredKeys.SelectMany(g => g))
                    DrawLine(canvas, $"    Key: {key.Name}", ref drawPt, _questKeyPaint);

            if (_showRequiredItems && !done && obj has item requirement)
                foreach (var itemId in obj.RequiredItemIds)
                    DrawLine(canvas, $"    Item: {GetItemName(itemId)}", ref drawPt, _questItemPaint);
        }
    }
}
```

### 9.4 Click handling for collapse toggles

The widget implements `HandleClientAreaClick(SKPoint point)`. It tracks Y positions of each rendered quest line and maps clicks on `[-]`/`[+]` prefixes to toggle `_collapsedQuests[quest.Id]`. Filter checkboxes on the top line are also clickable via X position measurement with `_questTextPaint.MeasureText(...)`.

---

## 10. ESP Rendering — World Quest Zones

`QuestLocation` implements the standard map entity interfaces and is rendered as a dot + label on the 2D radar map, and as a 3D marker in the ESP window.

### 10.1 Map rendering (2D radar)

- Draw a filled circle at the projected 2D map position.
- Color: configurable via `EntityTypeSettings` ("QuestZone").
- Label: `quest.QuestName` above the dot.
- If `Outline != null`: draw the polygon by projecting each `Vector3` vertex to 2D map space and connecting them with lines. This creates the zone boundary visible on the map.

### 10.2 ESP rendering (3D overlay)

- Project `Position` through the camera matrix to screen space.
- Draw a dot/icon at the screen position.
- Show `QuestName` as a label.
- Cull if behind camera or outside screen bounds.

### 10.3 Location conditions list

During `Refresh()`, all incomplete location-based objectives have their `QuestLocation` objects collected:

```csharp
foreach (var objective in quest.Objectives)
{
    if (!objective.IsCompleted)
        allLocationConditions.AddRange(objective.LocationObjectives);
}
```

The `LocationConditions` property exposes this list to the map renderer, which iterates it each frame to draw markers.

---

## 11. Configuration Flags

```csharp
class QuestHelperConfig
{
    bool Enabled;                          // master on/off switch
    bool KappaFilter;                      // show only Kappa-required quests
    bool OptionalTaskFilter;               // include optional sub-objectives
    bool KillZones;                        // show kill-zone outlines on map
    HashSet<string> BlacklistedQuests;     // quest IDs to hide from display
}
```

**KappaFilter** affects two things:
1. The memory read loop (`continue` if `!taskData.KappaRequired`)
2. The zone cache build (only includes Kappa quest zones)

**BlacklistedQuests** is checked during the memory read loop by quest ID, allowing per-quest suppression.

**OptionalTaskFilter** controls whether optional objectives appear in both the zone cache and the HUD widget.

---

## 12. Common Pitfalls

### 12.1 `QuestsData` can be null mid-raid
The game temporarily sets `QuestsData` to null during certain events (trader reward screen, quest completion animations). Always null-check and rate-limit retries; do not throw — just skip this refresh cycle.

### 12.2 `CompletedConditions` is null for quests with no progress
A `completedHashSetPtr == 0` is **normal** for a quest that has been started but no conditions completed yet. Treat it as an empty set, not an error.

### 12.3 Do not parse `IEnumerable<Condition>` from memory
`ConditionCollection._necessaryConditions` uses LINQ iterator objects in IL2CPP. Their internal layout (state machine fields, current element pointer) changes every patch and is extremely fragile to DMA-read. Use the static API data for all objective metadata.

### 12.4 HashSet count offset varies
The count field of `HashSet<T>` can live at `0x1C`, `0x20`, or `0x3C` depending on IL2CPP compiler version and struct alignment. Always try all three and take the first plausible result (> 0 and < 100 for quest conditions).

### 12.5 MongoID is a value type — no pointer indirection
`MongoID` is a `struct`, stored **inline** in the `HashSet.Entry`. There is no additional pointer dereference to get to the struct fields. The `_stringID` field at `+0x10` within the struct IS a pointer (to a Unity managed string object).

### 12.6 Zone zone ID mismatch
Zone IDs in the API (e.g., `"Customs_2_1_4"`) must exactly match the zone IDs returned in `apiObj.Zones[i].Id`. If they do not match the zone cache keys, no location markers will appear. Case-insensitive dictionary keys resolve most issues.

### 12.7 Map ID resolution
Always use the `_mapToId` translation dictionary. `Memory.MapID` returns internal game strings (`"bigmap"`, `"Sandbox"`). The tarkov.dev API uses BSG MongoIDs. Without this translation, zone lookups fail silently.

### 12.8 Rate limit is essential
Quest memory is read on the same thread as all other game-world reads. At 133ms/frame, calling full quest parsing every frame wastes significant DMA budget. A 2-second rate limit is appropriate — quest status changes are infrequent.

---

## 13. EQuestStatus Enum Reference

| Value | Name                 | Description                              |
|-------|----------------------|------------------------------------------|
| 0     | Locked               | Prerequisites not met                    |
| 1     | AvailableForStart    | Can be started from trader               |
| 2     | **Started**          | **Active — only these are tracked**      |
| 3     | AvailableForFinish   | All objectives done, hand-in pending     |
| 4     | Success              | Completed and handed in                  |
| 5     | Fail                 | Failed (timer expired or condition fail) |
| 6     | FailRestartable      | Failed but can be restarted              |
| 7     | MarkedAsFailed       | Admin-marked failed                      |
| 8     | Expired              | Time-limited quest expired               |
| 9     | AvailableAfter       | Unlocks after a time delay               |

Only filter for `Status == 2` (Started). All other statuses should be ignored.

---

## Quick Implementation Checklist

- [ ] Add `QuestsData = 0x98` to `Profile` offsets
- [ ] Add `QuestData` offsets struct: `Id=0x10`, `Status=0x1C`, `CompletedConditions=0x28`, `Template=0x38`
- [ ] Add `UnityOffsets` entries for `ManagedList`, `ManagedArray`, `IL2CPPHashSet2`, `MongoID`
- [ ] Integrate `EftDataManager.TaskData` (tarkov.dev API fetch at startup)
- [ ] Build `_questZones` and `_questOutlines` frozen dictionaries from API data at startup
- [ ] Implement `ReadHashSetMongoIds()` with all three count-offset fallbacks (`0x1C`, `0x20`, `0x3C`)
- [ ] Implement `QuestManager.Refresh()` with 2-second rate limit
- [ ] Implement `GetQuestsForCurrentMap()` using the map ID translation table
- [ ] Expose `LocationConditions` list for the map renderer
- [ ] Expose `IsItemRequired(itemId)` for loot highlighting
- [ ] Add `QuestLocation` as `IMapEntity` with optional polygon outline rendering
- [ ] Add `ESPQuestInfoWidget` / `QuestInfoWidget` HUD panels
- [ ] Add `QuestHelperConfig` to user config
- [ ] Wire `QuestManager` into `LocalGameWorld`; call `questManager.Refresh()` each game loop tick
