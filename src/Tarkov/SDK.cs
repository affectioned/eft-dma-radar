namespace SDK
{
    public readonly partial struct Offsets
    {
        //public static class TarkovApplication
        public readonly partial struct TarkovApplication
        {
            public static uint _menuOperation = 0x128;
        }
        //public static class MainMenuShowOperation
        public readonly partial struct MainMenuShowOperation
        {
            public static uint _preloaderUI = 0x60;
            public static uint _profile = 0x50;
        }
        //public static class PreloaderUI
        public readonly partial struct PreloaderUI
        {
            public static uint _sessionIdText = 0x118;
        }
        //public static class GameWorld
        public readonly partial struct GameWorld
        {
            public static uint SynchronizableObjectLogicProcessor = 0x248; //_SynchronizableObjectLogicProcessor_k__BackingField
        }
        //public static class GameWorld
        public readonly partial struct ClientLocalGameWorld
        {
            public static uint BtrController = 0x28; //_BtrController_k__BackingField
            public static uint TransitController = 0x38; //_TransitController_k__BackingField
            public static uint ExfilController = 0x58; //_ExfiltrationController_k__BackingField
            public static uint ClientShellingController = 0xA8; //_ClientShellingController_k__BackingField
            public static uint LocationId = 0xD0; //_LocationId_k__BackingField
            public static uint LootList = 0x198;
            public static uint RegisteredPlayers = 0x1B0;
            public static uint MainPlayer = 0x210;
            public static uint World = 0x218; //_world
            public static uint SynchronizableObjectLogicProcessor = 0x248; //_SynchronizableObjectLogicProcessor_k__BackingField
            public static uint Grenades = 0x288;
        }
        //public static class TransitController
        public readonly partial struct TransitController
        {
            public static uint TransitPoints = 0x18; //pointsById
        }
        //public static class ArtilleryShellingControllerClient
        public readonly partial struct ClientShellingController
        {
            public static uint ActiveClientProjectiles = 0x68;
        }
        //public static class World_2
        public readonly partial struct WorldController
        {
            public static uint Interactables = 0x30; //_interactables
        }
        //public static class WorldInteractiveObject
        public readonly partial struct Interactable
        {
            public static uint KeyId = 0x60;
            public static uint Id = 0x70;
            public static uint _doorState = 0xD0;
        }
        //public static class ArtilleryProjectileClient
        public readonly partial struct ArtilleryProjectileClient
        {
            public const uint Position = 0x30; //_targetPosition
            public const uint IsActive = 0x3C; //_flyOn
        }
        //public static class TransitPoint
        public readonly partial struct TransitPoint
        {
            public static uint parameters = 0x20;
        }
        //public static class TransitParameters
        public readonly partial struct TransitParameters
        {
            public static uint id = 0x10;
            public static uint active = 0x14;
            public static uint name = 0x18;
            public static uint description = 0x20;
            public static uint target = 0x38;
            public static uint location = 0x40;
        }
        //public static class SynchronizableObject
        public readonly partial struct SynchronizableObject
        {
            public static uint Type = 0x68;
        }
        //public static class SynchronizableObjectLogicProcessor
        public readonly partial struct SynchronizableObjectLogicProcessor
        {
            public static uint _activeSynchronizableObjects = 0x18;
        }
        //public static class TripwireSynchronizableObject
        public readonly partial struct TripwireSynchronizableObject
        {
            public static uint GrenadeTemplateId = 0x118; //_GrenadeTemplateId_k__BackingField
            public static uint _tripwireState = 0xE4;
            public static uint FromPosition = 0x14C; //_FromPosition_k__BackingField
            public static uint ToPosition = 0x158; //_ToPosition_k__BackingField
        }
        //public static class BtrController
        public readonly partial struct BtrController
        {
            public static uint BtrView = 0x50; //_BtrView_k__BackingField
        }
        //public static class BTRView
        public readonly partial struct BTRView
        {
            public static uint turret = 0x60;
            public static uint _previousPosition = 0xB4; //_previousPosition
        }
        //public static class BTRTurretView
        public readonly partial struct BTRTurretView
        {
            public static uint AttachedBot = 0x60; //_bot
        }
        //public static class HealthInfo
        public readonly partial struct HealthController
        {
            public static uint Energy = 0x68;
            public static uint Hydration = 0x70;
        }
        //public static class ExfiltrationController
        public readonly partial struct ExfilController
        {
            public static uint ExfiltrationPointArray = 0x20; //_ExfiltrationPoints_k__BackingField
            public static uint ScavExfiltrationPointArray = 0x28; //_ScavExfiltrationPoints_k__BackingField
            public static uint SecretExfiltrationPointArray = 0x30; //_SecretExfiltrationPoints_k__BackingField
        }
        //public static class ExfiltrationPoint
        public readonly partial struct Exfil
        {
            public static uint _status = 0x58;
            public static uint Settings = 0x98;
            public static uint EligibleEntryPoints = 0xC0;
        }
        //public static class ScavExfiltrationPoint
        public readonly partial struct ScavExfil
        {
            public static uint EligibleIds = 0xF8;
        }
        //public static class ExitTriggerSettings
        public readonly partial struct ExfilSettings
        {
            public static uint Name = 0x18;
        }
        //public static class Grenade + public static class Throwable
        public readonly partial struct Grenade
        {
            public static uint IsDestroyed = 0x4D; //_isDestroyed from Throwable
            public static uint WeaponSource = 0x98; //_WeaponSource_k__BackingField from Grenade
        }
        //public static class Player
        public readonly partial struct Player
        {
            public static uint _characterController = 0x40;
            public static uint MovementContext = 0x60; //_MovementContext_k__BackingField
            public static uint _playerBody = 0x190;
            public static uint ProceduralWeaponAnimation = 0x338; //_ProceduralWeaponAnimation_k__BackingField
            public static uint Corpse = 0x680;
            public static uint Location = 0x870; //_Location_k__BackingField
            public static uint Profile = 0x900; //_Profile_k__BackingField
            public static uint _healthController = 0x960;
            public static uint _inventoryController = 0x978;
            public static uint _handsController = 0x980;
            public static uint VoipID = 0x8F0; //_VoipID_k__BackingField
        }
        //public static class ObservedPlayerView
        public readonly partial struct ObservedPlayerView
        {
            public static uint ObservedPlayerController = 0x28; //_ObservedPlayerController_k__BackingField
            public static uint Voice = 0x40; //_Voice_k__BackingField
            public static uint GroupID = 0x80; //_GroupId_k__BackingField
            public static uint Side = 0x94; //_Side_k__BackingField
            public static uint IsAI = 0xA0; //_IsAI_k__BackingField
            public static uint AccountId = 0xC0; //_AccountId_k__BackingField
            public static uint PlayerBody = 0xD8; //_PlayerBody_k__BackingField
            public static uint VoipId = 0xB0; //_VoipID_k__BackingField
        }
        //public static class ObservedPlayerController
        public readonly partial struct ObservedPlayerController
        {
            public static uint InventoryController = 0x10; //_InventoryController_k__BackingField
            public static uint Player = 0x18; //_PlayerView_k__BackingField
            public static readonly uint[] MovementController = new uint[] { 0xD8, 0x98 }; //_MovementController_k__BackingField
            public static uint HealthController = 0xE8; //_HealthController_k__BackingField
            public static uint HandsController = 0x120; //_HandsController_k__BackingField
        }
        //public static class ObservedPlayerStateContext
        public readonly partial struct ObservedMovementController
        {
            public static uint Rotation = 0x20; //_Rotation_k__BackingField
        }
        //public static class ObservedPlayerHandsController
        public readonly partial struct ObservedHandsController
        {
            public static uint ItemInHands = 0x58; //_item
            public static uint BundleAnimationBones = 0xA8; //_bundleAnimationBones
        }
        //public static class BundleAnimationBones
        public readonly partial struct BundleAnimationBonesController
        {
            public static uint ProceduralWeaponAnimationObs = 0xD0; //_ProceduralWeaponAnimation_k__BackingField
        }
        //public static class ProceduralWeaponAnimation
        public readonly partial struct ProceduralWeaponAnimationObs
        {
            public static uint _isAimingObs = 0x145; //_isAiming
        }
        //public static class ObservedPlayerHealthController
        public readonly partial struct ObservedHealthController
        {
            public static uint Player = 0x18; //_player
            public static uint PlayerCorpse = 0x20; //_playerCorpse
            public static uint HealthStatus = 0x10;
        }
        //public static class ProceduralWeaponAnimation
        public readonly partial struct ProceduralWeaponAnimation
        {
            public static uint _isAiming = 0x145;
            public static uint _optics = 0x180;
        }
        //public static class SightNBone
        public readonly partial struct SightNBone
        {
            public static uint Mod = 0x10;
        }
        //public static class Profile
        public readonly partial struct Profile
        {
            public static uint Id = 0x10;
            public static uint AccountId = 0x18;
            public static uint Info = 0x48;
            public static uint Inventory = 0x70;
            public static uint QuestsData = 0x98;
            public static uint WishlistManager = 0x108;
        }
        //public static class WishlistManager
        public readonly partial struct WishlistManager
        {
            public static uint Items = 0x28; //_userItems
        }
        //public static class ProfileInfo
        public readonly partial struct PlayerInfo
        {
            public static uint EntryPoint = 0x28;
            public static uint Side = 0x48; //_Side_k__BackingField
            public static uint RegistrationDate = 0x4C;
            public static uint GroupId = 0x50;
        }
        //public static class QuestStatusData
        public readonly partial struct QuestData
        {
            public static uint Id = 0x10;
            public static uint Status = 0x1C;
            public static uint CompletedConditions = 0x28;
            public static uint Template = 0x38;
        }
        //public static class CompletedConditionsCollection
        public readonly partial struct CompletedConditionsCollection
        {
            public static uint BackendData = 0x10; //_backendData
            public static uint LocalChanges = 0x18; //_localChanges
        }
        //public static class QuestTemplate
        public readonly partial struct QuestTemplate
        {
            public static uint Conditions = 0x60; //_Conditions_k__BackingField
            public static uint Name = 0xC8; //_questName
        }
        //?
        public readonly partial struct QuestConditionsContainer
        {
            public static uint ConditionsList = 0x70;
        }
        //public static class ItemHandsController
        public readonly partial struct ItemHandsController
        {
            public static uint Item = 0x70; //_item
        }
        //public static class FirearmController
        public readonly partial struct FirearmController
        {
            public static uint Fireport = 0x150;
        }
        //public static class MovementContext
        public readonly partial struct MovementContext
        {
            public static uint Player = 0x48; //_player
            public static uint _rotation = 0xC8;
            public static uint CurrentState = 0x1F0; //_CurrentState_k__BackingField
        }
        //public static class InventoryController
        public readonly partial struct InventoryController
        {
            public static uint Inventory = 0x100; //_Inventory_k__BackingField
        }
        //public static class Inventory
        public readonly partial struct Inventory
        {
            public static uint Equipment = 0x18;
            public static uint Stash = 0x20;
        }
        //public static class Stash + ? might be using public static class CompoundItem
        public readonly partial struct Stash
        {
            public static uint Grids = 0x98; //_grid
        }
        //public static class CompoundItem
        public readonly partial struct Equipment
        {
            public static uint Slots = 0x80;
        }
        //public static class BarterOther
        public readonly partial struct BarterOtherOffsets
        {
            public static uint Dogtag = 0x80;
        }
        //public static class DogtagComponent
        public readonly partial struct DogtagComponent
        {
            public static uint AccountId = 0x20;
            public static uint ProfileId = 0x28;
            public static uint Nickname = 0x30;
            public static uint KillerAccountId = 0x50;
            public static uint KillerProfileId = 0x58;
            public static uint KillerName = 0x60;
            public static uint WeaponName = 0x68;
        }
        //public static class Grid
        public readonly partial struct Grids
        {
            public static uint ContainedItems = 0x48; //_ItemCollection_k__BackingField
        }
        //public static class GridItemCollection
        public readonly partial struct GridContainedItems
        {
            public static uint Items = 0x18; //ItemsList
        }
        //public static class Slot
        public readonly partial struct Slot
        {
            public static uint ContainedItem = 0x48; //_ContainedItem_k__BackingField
            public static uint ID = 0x58; //_ID_k__BackingField
        }
        //public static class LootItem
        public readonly partial struct InteractiveLootItem
        {
            public static uint Item = 0xF0; //_item
        }
        //public static class Skeleton
        public readonly partial struct DizSkinningSkeleton
        {
            public static uint _values = 0x30;
        }
        //public static class LootableContainer + public static class WorldInteractiveObject
        public readonly partial struct LootableContainer
        {
            public static uint InteractingPlayer = 0x150; //from WorldInteractiveObject _InteractingPlayer_k__BackingField
            public static uint ItemOwner = 0x168;
        }
        //public static class ItemController
        public readonly partial struct LootableContainerItemOwner
        {
            public static uint RootItem = 0xD0; //_RootItem_k__BackingField
        }
        //public static class Item
        public readonly partial struct LootItem
        {
            public static uint StackObjectsCount = 0x24;
            public static uint Version = 0x28;
            public static uint Components = 0x40;
            public static uint Template = 0x60; //_Template_k__BackingField
            public static uint SpawnedInSession = 0x68; //_SpawnedInSession_k__BackingField
        }
        //public static class CompoundItem
        public readonly partial struct LootItemMod
        {
            public static uint Grids = 0x78;
            public static uint Slots = 0x80;
        }
        //public static class Grid
        public readonly partial struct Grid
        {
            public static uint ItemCollection = 0x48; //_ItemCollection_k__BackingField
        }
        //public static class GridItemCollection
        public readonly partial struct GridItemCollection
        {
            public static uint ItemsList = 0x18;
        }
        //public static class Weapon
        public readonly partial struct LootItemWeapon
        {
            public static uint FireMode = 0xA0;
            public static uint Chambers = 0xB0; //_Chambers_k__BackingField
            public static uint _magSlotCache = 0xC8;
        }
        //public static class FireModeComponent
        public readonly partial struct FireModeComponent
        {
            public static uint FireMode = 0x28;
        }
        //public static class MagazineTemplate
        public readonly partial struct LootItemMagazine
        {
            public static uint Cartridges = 0xA8;
        }
        //public static class Item
        public readonly partial struct MagazineClass
        {
            public static uint StackObjectsCount = 0x24;
        }
        //public static class StackSlot
        public readonly partial struct StackSlot
        {
            public static uint _items = 0x18;
            public static uint MaxCount = 0x10;
        }
        //public static class ItemTemplate
        public readonly partial struct ItemTemplate
        {
            public static uint Name = 0x10;
            public static uint ShortName = 0x18;
            public static uint _id = 0xE0; //__id_k__BackingField
            public static uint QuestItem = 0x34;
        }
        //public static class PlayerBody
        public readonly partial struct PlayerBody
        {
            public static uint SkeletonRootJoint = 0x30;
        }
        //public static class OpticCameraManager
        public readonly partial struct OpticCameraManager
        {
            public static uint Camera = 0x70; //_Camera_k__BackingField
        }
        //public static class CameraManager
        public readonly partial struct EFTCameraManager
        {
            public static uint OpticCameraManager = 0x10; //_OpticCameraManager_k__BackingField
            public static uint Camera = 0x60; //_Camera_k__BackingField
            public static uint GetInstance_RVA = 0x2CF8AB0; //get_Instance_RVA
            public static uint CameraDerefOffset = 0x10;
        }
        //public static class SightComponent
        public readonly partial struct SightComponent
        {
            public const uint _template = 0x20;
            public const uint ScopesSelectedModes = 0x30;
            public const uint SelectedScope = 0x38;
            public const uint ScopeZoomValue = 0x3C;
        }
        //public static class SightModTemplate
        public readonly partial struct SightInterface
        {
            public const uint Zooms = 0x1B8;
        }
        //
        public readonly partial struct Special
        {
            public static ulong TypeInfoTableRva = 0x00000;
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