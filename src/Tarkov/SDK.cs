namespace SDK
{
    public readonly partial struct Offsets
    {
        public static class AssemblyCSharp
        {
            public const uint TypeStart = 0;
            public const uint TypeCount = 16336;
        }
        public readonly partial struct TarkovApplication
        {
            public const uint _menuOperation = 0x130; // -.\uEA0Fget
        }

        public readonly partial struct MainMenuShowOperation
        {
            public const uint _preloaderUI = 0x60; // -.\uEA07
            public const uint _profile = 0x50; // -.\uEA07
        }
        public readonly partial struct PreloaderUI
        {
            public const uint _sessionIdText = 0x118;
        }

        public readonly partial struct GameWorld
        {
            public const uint SynchronizableObjectLogicProcessor = 0x248; // <SynchronizableObjectLogicProcessor>k__BackingField (5 March, 2026)
        }

        public readonly partial struct ClientLocalGameWorld
        {
            public const uint BtrController = 0x28; // BtrController (5 March, 2026)
            public const uint TransitController = 0x38; // TransitController (5 March, 2026)
            public const uint ExfilController = 0x58; // ExfiltrationController (5 March, 2026)
            public const uint ClientShellingController = 0xA8; // ArtilleryShellingControllerClient (5 March, 2026)
            public const uint LocationId = 0xD0; // String (5 March, 2026)
            public const uint LootList = 0x198; // List<IKillable> (5 March, 2026)
            public const uint RegisteredPlayers = 0x1B8; // List<EFT.IPlayer> (5 March, 2026)
            public const uint MainPlayer = 0x210; // EFT.Player (5 March, 2026)
            public const uint World = 0x218; // EFT.World (5 March, 2026)
            public const uint Grenades = 0x288; // DictionaryListHydra<Int32, Throwable> (5 March, 2026)
        }

        public readonly partial struct TransitController
        {
            public const uint TransitPoints = 0x18; // System.Collections.Generic.Dictionary<Int32, TransitPoint> (DEC 3) - pointsById field
        }

        public readonly partial struct ClientShellingController
        {
            public const uint ActiveClientProjectiles = 0x68; // System.Collections.Generic.Dictionary<Int32, ArtilleryProjectileClient> (DEC 3)
        }

        public readonly partial struct WorldController
        {
            public const uint Interactables = 0x30; // EFT.Interactive.WorldInteractiveObject[] (UNKN)
        }

        public readonly partial struct Interactable
        {
            public const uint KeyId = 0x60; // String (DEC 3)
            public const uint Id = 0x70; // String (DEC 3)
            public const uint _doorState = 0xD0; // EDoorState (DEC 3)
        }

        public readonly partial struct ArtilleryProjectileClient
        {
            public const uint Position = 0x30; // UnityEngine.Vector3 (DEC 3) - _targetPosition field
            public const uint IsActive = 0x3C; // Boolean (DEC 3) - _flyOn field
        }

        public readonly partial struct TransitPoint
        {
            public const uint parameters = 0x20; // -.\uE6CC.Location.TransitParameters (DEC 3)
        }

        public readonly partial struct TransitParameters
        {
            public const uint location = 0x40; // String (DEC 3) - FIXED from 0x30
        }

        public readonly partial struct SynchronizableObject
        {
            public const uint Type = 0x68; // SynchronizableObjectType (DEC 3)
        }

        public readonly partial struct SynchronizableObjectLogicProcessor
        {
            public const uint _activeSynchronizableObjects = 0x18; // _activeSynchronizableObjects
        }

        public readonly partial struct TripwireSynchronizableObject
        {
            public const uint GrenadeTemplateId = 0x118; // EFT.MongoID (5 March, 2026) - <GrenadeTemplateId>k__BackingField
            public const uint _tripwireState = 0xE4; // ETripwireState (DEC 3)
            public const uint FromPosition = 0x14C; // UnityEngine.Vector3 (DEC 3) - <FromPosition>k__BackingField
            public const uint ToPosition = 0x158; // UnityEngine.Vector3 (DEC 3) - <ToPosition>k__BackingField
        }

        public readonly partial struct BtrController
        {
            public const uint BtrView = 0x50; // EFT.Vehicle.BTRView (DEC 3) - <BtrView>k__BackingField
        }

        public readonly partial struct BTRView
        {
            public const uint turret = 0x60; // EFT.Vehicle.BTRTurretView (DEC 3)
            public const uint _previousPosition = 0xB4; // UnityEngine.Vector3 (DEC 3)
        }

        public readonly partial struct BTRTurretView
        {
            public const uint AttachedBot = 0x60; // System.ValueTuple<ObservedPlayerView, Boolean> (DEC 3) - _bot field
        }

        /// <summary>
        /// EFT.HealthSystem.ActiveHealthController/BaseHealthController - For LocalPlayer's health (IL2CPP)
        /// </summary>
        public readonly partial struct HealthController
        {
            public const uint Energy = 0x68; // IL2CPP DEC 2025 (from Camera-PWA)
            public const uint Hydration = 0x70; // IL2CPP DEC 2025 (from Camera-PWA)
        }

        public readonly partial struct ExfilController
        {
            public const uint ExfiltrationPointArray = 0x20; // ExfiltrationPoint[] (DEC 3) - <ExfiltrationPoints>k__BackingField
            public const uint ScavExfiltrationPointArray = 0x28; // ScavExfiltrationPoint[] (DEC 3) - <ScavExfiltrationPoints>k__BackingField
            public const uint SecretExfiltrationPointArray = 0x30; // SecretExfiltrationPoint[] (DEC 3) - <SecretExfiltrationPoints>k__BackingField
        }

        public readonly partial struct Exfil
        {
            public const uint _status = 0x58; // EExfiltrationStatus (DEC 1)
            public const uint Settings = 0x98; // ExitTriggerSettings (DEC 1)
            public const uint EligibleEntryPoints = 0xC0; // String[] (DEC 1)
        }

        public readonly partial struct ScavExfil
        {
            public const uint EligibleIds = 0xF8; // List<String>
        }

        public readonly partial struct ExfilSettings
        {
            public const uint Name = 0x18; // String
        }

        public readonly partial struct Grenade
        {
            public const uint IsDestroyed = 0x4D; // Boolean (DEC 1)
            public const uint WeaponSource = 0x80; // -.\uEF81 (UNKN)
        }

        public readonly partial struct Player
        {
            public const uint _characterController = 0x40; // ICharacterController (DEC 3)
            public const uint MovementContext = 0x60; // MovementContext (DEC 3)
            public const uint _playerBody = 0x190; // PlayerBody (DEC 3)
            public const uint ProceduralWeaponAnimation = 0x338; // ProceduralWeaponAnimation (DEC 3)
            public const uint Corpse = 0x680; // Corpse (DEC 3)
            public const uint Location = 0x870; // String (DEC 3)
            public const uint Profile = 0x900; // Profile (DEC 3)
            public const uint _healthController = 0x960; // IHealthController (DEC 3)
            public const uint _inventoryController = 0x978; // PlayerInventoryController (DEC 3)
            public const uint _handsController = 0x980; // AbstractHandsController (DEC 3)
            public const uint VoipID = 0x8f0; // Boolean (DEC 3)
        }

        public readonly partial struct ObservedPlayerView
        {
            public const uint ObservedPlayerController = 0x28; // ObservedPlayerController (DEC 3) - <ObservedPlayerController>k__BackingField
            public const uint Voice = 0x40; // String (DEC 3) - <Voice>k__BackingField
            public const uint GroupID = 0x80; // String (DEC 3) - <GroupId>k__BackingField
            public const uint Side = 0x94; // EPlayerSide (DEC 3) - <Side>k__BackingField
            public const uint IsAI = 0xa0; // Boolean (DEC 3) - <IsAI>k__BackingField
            public const uint NickName = 0xb8; // String (DEC 3) - <NickName>k__BackingField
            public const uint AccountId = 0xc0; // String (DEC 3) - <AccountId>k__BackingField
            public const uint PlayerBody = 0xd8; // PlayerBody (DEC 3) - <PlayerBody>k__BackingField
            public const uint VoipId = 0xB0;
        }

        public readonly partial struct ObservedPlayerController
        {
            public const uint InventoryController = 0x10; // ObservedPlayerInventoryController (DEC 3) - <InventoryController>k__BackingField
            public const uint Player = 0x18; // ObservedPlayerView (DEC 3) - <PlayerView>k__BackingField
            public static readonly uint[] MovementController = new uint[] { 0xD8, 0x98 }; // ObservedPlayerMovementController, ObservedPlayerStateContext (DEC 3) - <MovementController>k__BackingField, then <ObservedPlayerStateContext>k__BackingField
            public const uint HealthController = 0xE8; // ObservedPlayerHealthController (DEC 3) - <HealthController>k__BackingField
            public const uint HandsController = 0x120; // ObservedPlayerHandsController (DEC 3) - <HandsController>k__BackingField
        }

        public readonly partial struct ObservedMovementController
        {
            public const uint Rotation = 0x20; // UnityEngine.Vector2 (DEC 3) - <Rotation>k__BackingField in ObservedPlayerStateContext
        }

        public readonly partial struct ObservedHandsController
        {
            public const uint ItemInHands = 0x58; // EFT.InventoryLogic.Item (DEC 3) - _item field
            public const uint BundleAnimationBones = 0xA8; // BundleAnimationBones (DEC 3) - _bundleAnimationBones field
        }

        public readonly partial struct BundleAnimationBonesController
        {
            public const uint ProceduralWeaponAnimationObs = 0xD0; // EFT.Animations.ProceduralWeaponAnimation (UNKN)
        }

        public readonly partial struct ProceduralWeaponAnimationObs
        {
            public const uint _isAimingObs = 0x145; // Boolean (UNKN)
        }

        public readonly partial struct ObservedHealthController
        {
            public const uint Player = 0x18; // EFT.NextObservedPlayer.ObservedPlayerView (DEC 3) - _player field
            public const uint PlayerCorpse = 0x20; // EFT.Interactive.ObservedCorpse (DEC 3) - _playerCorpse field
            public const uint HealthStatus = 0x10; // ETagStatus (DEC 3)
        }

        public readonly partial struct ProceduralWeaponAnimation
        {
            public const uint _isAiming = 0x145;
            public const uint _optics = 0x180;
        }

        public readonly partial struct SightNBone
        {
            public const uint Mod = 0x10; // EFT.InventoryLogic.SightComponent (DEC 1)
        }

        public readonly partial struct Profile
        {
            public const uint Id = 0x10; // String (DEC 1)
            public const uint AccountId = 0x18; // String (DEC 1)
            public const uint Info = 0x48; // ProfileInfo (DEC 1)
            public const uint Inventory = 0x70; // SkillManager (DEC 1)
            public const uint QuestsData = 0x98; // List<QuestStatusData> (DEC 1)
            public const uint WishlistManager = 0x108; // WishlistManager (DEC 1)
        }

        public readonly partial struct WishlistManager
        {
            public const uint Items = 0x28; // System.Collections.Generic.Dictionary<MongoID, Int32> (UNKN)
        }

        public readonly partial struct PlayerInfo
        {
            public const uint EntryPoint = 0x28; // String (DEC 1)
            public const uint Side = 0x48; // EPlayerSide (DEC 1)
            public const uint RegistrationDate = 0x4C; // Int32 (DEC 1)
            public const uint GroupId = 0x50; // String (DEC 1)
        }

        public readonly partial struct QuestData
        {
            // QuestStatusData in IL2CPP dump
            public const uint Id = 0x10; // String
            public const uint Status = 0x1c; // EQuestStatus (IL2CPP - was 0x34 in Mono)
            public const uint CompletedConditions = 0x28; // CompletedConditionsCollection (IL2CPP - was 0x20 in Mono)
            public const uint Template = 0x38; // QuestTemplate (IL2CPP - was 0x28 in Mono)
        }

        /// <summary>
        /// CompletedConditionsCollection structure (IL2CPP)
        /// Contains backend and local HashSet&lt;MongoID&gt; for quest conditions
        /// </summary>
        public readonly partial struct CompletedConditionsCollection
        {
            public const uint BackendData = 0x10;   // HashSet<MongoID> - conditions from server
            public const uint LocalChanges = 0x18;  // HashSet<MongoID> - conditions completed this raid
        }

        public readonly partial struct QuestTemplate
        {
            // QuestTemplate in IL2CPP dump
            public const uint Conditions = 0x60; // ConditionsDict (IL2CPP - was 0x40 in Mono)
            public const uint Name = 0xC8; // String (_questName in dump)
        }

        public readonly partial struct QuestConditionsContainer
        {
            // ConditionCollection - the list is accessed via _necessaryConditions
            public const uint ConditionsList = 0x70; // IEnumerable<Condition> (IL2CPP - was 0x50 in Mono)
        }

        public readonly partial struct ItemHandsController
        {
            public const uint Item = 0x70; // EFT.InventoryLogic.Item (DEC 1)
        }

        public readonly partial struct FirearmController
        {
            public const uint Fireport = 0x150; // EFT.BifacialTransform (DEC 3)
        }

        public readonly partial struct MovementContext
        {
            public const uint Player = 0x48; // EFT.Player
            public const uint _rotation = 0xc8; // UnityEngine.Vector2
            public const uint CurrentState = 0x1F0; // EFT.BaseMovementState <CurrentState>k__BackingField
        }

        public readonly partial struct InventoryController
        {
            public const uint Inventory = 0x100; // EFT.InventoryLogic.Inventory
        }

        public readonly partial struct Inventory
        {
            public const uint Equipment = 0x18; // EFT.InventoryLogic.InventoryEquipment
            public const uint Stash = 0x20; // -.\uEFFE (UNKN)
        }

        public readonly partial struct Stash
        {
            public const uint Grids = 0x98; // -.\uEFFE (UNKN)
        }

        public readonly partial struct Equipment
        {
            public const uint Slots = 0x80; // EFT.InventoryLogic.Slot[] (UNKN)
        }
        public readonly partial struct BarterOtherOffsets
        {
            public const uint Dogtag = 0x80; // EFT.InventoryLogic.BarterOther.Dogtag
        }
        public readonly partial struct DogtagComponent
        {
            public const uint AccountId = 0x20; // string
            public const uint ProfileId = 0x28; // string
            public const uint Nickname = 0x30; // string
            public const uint KillerAccountId = 0x50; // string
            public const uint KillerProfileId = 0x58; // string
            public const uint KillerName = 0x60; // string
            public const uint WeaponName = 0x68; // string
        }

        public readonly partial struct Grids
        {
            public const uint ContainedItems = 0x48; // -.\uEE76
        }

        public readonly partial struct GridContainedItems
        {
            public const uint Items = 0x18; // System.Collections.Generic.List<Item>
        }

        public readonly partial struct Slot
        {
            public const uint ContainedItem = 0x48; // EFT.InventoryLogic.Item (DEC 1)
            public const uint ID = 0x58; // String (DEC 1)
        }

        public readonly partial struct InteractiveLootItem
        {
            public const uint Item = 0xF0; // EFT.InventoryLogic.Item (DEC 1)
        }

        public readonly partial struct DizSkinningSkeleton
        {
            public const uint _values = 0x30; // System.Collections.Generic.List<Transform> (DEC 1)
        }

        public readonly partial struct LootableContainer
        {
            public const uint InteractingPlayer = 0x150; // EFT.IPlayer (UNKN)
            public const uint ItemOwner = 0x168; // -.\uEFB4 (DEC 1)
        }

        public readonly partial struct LootableContainerItemOwner
        {
            public const uint RootItem = 0xD0; // EFT.InventoryLogic.Item
        }

        public readonly partial struct LootItem
        {
            public const uint Template = 0x60; // ItemTemplate
        }

        public readonly partial struct LootItemMod
        {
            public const uint Grids = 0x78; // -.\uEE74[] (UNKN)
            public const uint Slots = 0x80; // EFT.InventoryLogic.Slot[] (UNKN)
        }
        public readonly partial struct Grid
        {
            public const uint ItemCollection = 0x48; // EFT.InventoryLogic.Slot[] (UNKN)
        }
        public static class GridItemCollection
        {
            public const uint ItemsList = 0x18; // List<EFT.InventoryLogic.Item>
        }
        public readonly partial struct LootItemWeapon
        {
            public const uint FireMode = 0xA0; // EFT.InventoryLogic.FireModeComponent
            public const uint Chambers = 0xB0; // EFT.InventoryLogic.Slot[]
            public const uint _magSlotCache = 0xC8; // EFT.InventoryLogic.Slot
        }

        public readonly partial struct EFT
        {
            public readonly partial struct EFTHardSettings
            {
                public const uint _instance = 0x0;
            }

            public static class WeatherController
            {
                public const uint Instance = 0x0;
            }

            public static class GPUInstancerManager
            {
                public const uint Instance = 0x0;
            }
        }

        public readonly partial struct FireModeComponent
        {
            public const uint FireMode = 0x28; // System.Byte (UNKN)
        }

        public readonly partial struct LootItemMagazine
        {
            public const uint Cartridges = 0xA8; // EFT.InventoryLogic.Magazine.<Cartridges>k__BackingField
        }

        public readonly partial struct MagazineClass
        {
            public const uint StackObjectsCount = 0x24; // EFT.InventoryLogic.Item.StackObjectsCount
        }

        public readonly partial struct StackSlot
        {
            public const uint _items = 0x18; // System.Collections.Generic.List<Item> (DEC 1)
            public const uint MaxCount = 0x10; // Int32 (DEC 1)
        }

        public readonly partial struct ItemTemplate
        {
            public const uint Name = 0x10; // String (DEC 1)
            public const uint ShortName = 0x18; // String (DEC 1)
            public const uint _id = 0xE0; // EFT.MongoID (DEC 1)
            public const uint QuestItem = 0x34; // Boolean (DEC 1)
        }

        public readonly partial struct PlayerBody
        {
            public const uint SkeletonRootJoint = 0x30; // Diz.Skinning.Skeleton (DEC 1)
        }

        public readonly partial struct OpticCameraManager
        {
            public const uint Camera = 0x70; // UnityEngine.Camera (DEC 1)
        }

        /// <summary>
        /// EFT.CameraControl.CameraManager - Main camera manager singleton (IL2CPP)
        /// </summary>
        public readonly partial struct EFTCameraManager
        {
            public const uint OpticCameraManager = 0x10; // UNCHANGED DEC 2025
            public const uint Camera = 0x60; // UnityEngine.Camera - FPS Camera (UNCHANGED DEC 2025)
            public const uint GetInstance_RVA = 0x3921890; // 5 March, 2026 - from Camera-PWA
            public const uint CameraDerefOffset = 0x10; // UNCHANGED DEC 2025 - dereference offset for Camera objects
        }

        public readonly partial struct SightComponent
        {
            public const uint _template = 0x20; // -.\uEE6C (DEC 1)
            public const uint ScopesSelectedModes = 0x30; // System.Int32[] (DEC 1)
            public const uint SelectedScope = 0x38; // Int32 (DEC 1)
            public const uint ScopeZoomValue = 0x3C; // Single (DEC 1)
        }

        public readonly partial struct SightInterface
        {
            public const uint Zooms = 0x1B8; // System.Single[]
        }
        public readonly partial struct WeatherController
        {
            public const uint WeatherDebug = 0x88;
        }
        public static class Special
        {
            public const ulong TypeInfoTableRva = 0x5AA9158; // TYPE_INFO_TABLE //updated feb11 (5 March, 2026)
        }
        public readonly partial struct Il2CppClass
        {
            public const uint Name = 0x10;
            public const uint Namespace = 0x18;
            public const uint StaticFields = 0xB8;
        }
    }

    public readonly partial struct Enums
    {
        public enum EPlayerState
        {
            None = 0,
            Idle = 1,
            ProneIdle = 2,
            ProneMove = 3,
            Run = 4,
            Sprint = 5,
            Jump = 6,
            FallDown = 7,
            Transition = 8,
            BreachDoor = 9,
            Loot = 10,
            Pickup = 11,
            Open = 12,
            Close = 13,
            Unlock = 14,
            Sidestep = 15,
            DoorInteraction = 16,
            Approach = 17,
            Prone2Stand = 18,
            Transit2Prone = 19,
            Plant = 20,
            Stationary = 21,
            Roll = 22,
            JumpLanding = 23,
            ClimbOver = 24,
            ClimbUp = 25,
            VaultingFallDown = 26,
            VaultingLanding = 27,
            BlindFire = 28,
            IdleWeaponMounting = 29,
            IdleZombieState = 30,
            MoveZombieState = 31,
            TurnZombieState = 32,
            StartMoveZombieState = 33,
            EndMoveZombieState = 34,
            DoorInteractionZombieState = 35,
        }

        [Flags]
        public enum EMemberCategory
        {
            Default = 0,
            Developer = 1,
            UniqueId = 2,
            Trader = 4,
            Group = 8,
            System = 16,
            ChatModerator = 32,
            ChatModeratorWithPermanentBan = 64,
            UnitTest = 128,
            Sherpa = 256,
            Emissary = 512,
            Unheard = 1024,
        }

        public enum WildSpawnType
        {
            marksman = 0,
            assault = 1,
            bossTest = 2,
            bossBully = 3,
            followerTest = 4,
            followerBully = 5,
            bossKilla = 6,
            bossKojaniy = 7,
            followerKojaniy = 8,
            pmcBot = 9,
            cursedAssault = 10,
            bossGluhar = 11,
            followerGluharAssault = 12,
            followerGluharSecurity = 13,
            followerGluharScout = 14,
            followerGluharSnipe = 15,
            followerSanitar = 16,
            bossSanitar = 17,
            test = 18,
            assaultGroup = 19,
            sectantWarrior = 20,
            sectantPriest = 21,
            bossTagilla = 22,
            followerTagilla = 23,
            exUsec = 24,
            gifter = 25,
            bossKnight = 26,
            followerBigPipe = 27,
            followerBirdEye = 28,
            bossZryachiy = 29,
            followerZryachiy = 30,
            bossBoar = 32,
            followerBoar = 33,
            arenaFighter = 34,
            arenaFighterEvent = 35,
            bossBoarSniper = 36,
            crazyAssaultEvent = 37,
            peacefullZryachiyEvent = 38,
            sectactPriestEvent = 39,
            ravangeZryachiyEvent = 40,
            followerBoarClose1 = 41,
            followerBoarClose2 = 42,
            bossKolontay = 43,
            followerKolontayAssault = 44,
            followerKolontaySecurity = 45,
            shooterBTR = 46,
            bossPartisan = 47,
            spiritWinter = 48,
            spiritSpring = 49,
            peacemaker = 50,
            pmcBEAR = 51,
            pmcUSEC = 52,
            skier = 53,
            sectantPredvestnik = 57,
            sectantPrizrak = 58,
            sectantOni = 59,
            infectedAssault = 60,
            infectedPmc = 61,
            infectedCivil = 62,
            infectedLaborant = 63,
            infectedTagilla = 64,
            bossTagillaAgro = 65,
            bossKillaAgro = 66,
            tagillaHelperAgro = 67,
        }

        public enum EExfiltrationStatus
        {
            NotPresent = 1,
            UncompleteRequirements = 2,
            Countdown = 3,
            RegularMode = 4,
            Pending = 5,
            AwaitsManualActivation = 6,
            Hidden = 7,
        }

        [Flags]
        public enum EProceduralAnimationMask
        {
            Breathing = 1,
            Walking = 2,
            MotionReaction = 4,
            ForceReaction = 8,
            Shooting = 16,
            DrawDown = 32,
            Aiming = 64,
            HandShake = 128,
        }

        public enum SynchronizableObjectType
        {
            AirDrop = 0,
            AirPlane = 1,
            Tripwire = 2,
        }

        public enum ETripwireState
        {
            None = 0,
            Wait = 1,
            Active = 2,
            Exploding = 3,
            Exploded = 4,
            Inert = 5,
        }

    }
}
