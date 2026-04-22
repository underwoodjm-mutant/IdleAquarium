using System;
using UnityEngine;

public enum inputMode
{
    keyboardMouse,
    controller,
    unknown
}

//These are the supported event types for the collection system
//Add more types as needed
public enum CollectionEventType
{
    donate, unknown
}

public enum platform
{
    steam,
    epic,
    sony,
    microsoft,
    nintendo,
    unknown
}

public enum menuMode
{
    mainMenu,
    settings,
    startGame,
    fileSelect,
    fileDetail,
    inGame,
    tutorial,
    unknown
}

public enum resourceType
{
    meat,
    bug,
    feather,
    plant,
    crate,
    barrel,
    unknown
}

public enum tutorialAcknowledgementButton
{
    A,
    X,
    B,
    Y,
    RThumbRot,
    LThumbRot,
    RThumbPress,
    LThumbPress,
    DpadU,
    DpadD,
    DpadL,
    DpadR,
    RT,
    LT,
    RS,
    LS,
    CenterL,
    CenterMid,
    CenterR,
    Unknown
}

public enum FishInfoEventType
{
    UNDISCOVERED, DISCOVERED
}

public enum KaijuDexTabChoice
{
    DEX, INVENTORY, MAP, QUESTS, UNKNOWN
}

public enum KaijuDexCategory
{
    MAIN, GENERAL, TROPICAL, ARCTIC, LAVA, SMALL, MEDIUM, LARGE, KAIJU, UNKNOWN
}

public enum KaijuDexChoice
{
    SECRETS, FISH, UNKNOWN
}

public enum gameDifficulty
{
    Easy,
    Normal,
    Hard,
    Extreme,
    Unknown
}

public enum vfxEventType
{
    Play,
    Stop
}

public enum damageType
{
    Electric,
    Fire,
    Ice,
    Normal,
    Unknown
}

public static class Common 
{
    public struct DebugSettings
    {
        public const bool DEBUG_MODE = false;
        public const bool GOD_MODE = false;
        public const bool NO_AI = false;
        public const string QUICK_DEV = "quickDevMode";
    }

    public struct BoatSettings
    {
        public enum BoatType { Small, Medium, Large}
        public const int smallStatEquipSlotCount = 3;
        public const int mediumStatEquipSlotCount = 5;
        public const int largeStatEquipSlotCount = 7;
    }

    public struct StatAttachmentValues
    {
        //Higher = faster boar
        public const int EnginePower_Min = -50;
        public const int EnginePower_Max = 50;

        //higher = faster turning
        public const float TurnPower_Min = -2;
        public const float TurnPower_Max = 2;

        //lower = faster acceleration
        public const float AccelerationTime_Min = 2;
        public const float AccelerationTime_Max = -2;

        //higher = faster reeling
        public const float ReelPower_Min = -2;
        public const float ReelPower_Max = 2;

        //higher = more tension before line break
        public const float LineStrength_Min = -2;
        public const float LineStrength_Max = 2;

        //Chance of different stat Attachment rarities. Needs to equal 100
        public enum StatsAttachmentRarity { Common, Uncommon, Rare, Epic, Legendary}
        public const float Chance_Common = 60;
        public const float Chance_Uncommon = 22;
        public const float Chance_Rare = 10;
        public const float Chance_Epic = 5;
        public const float Chance_Legendary = 3;
    }

    public struct FishSubTypes
    {
        //These must be synched up with the Fish.csv Name column
        public const string small1 = "Flamander"; 
        public const string small2 = "Tadfish"; 
        public const string small3 = "PuffPuff";

        public const string med1 = "Clampjaw1";
        public const string med2 = "Clampjaw2";
        public const string med3 = "Clampjaw3";

        public const string large1 = "BigShark1";
        public const string large2 = "BigShark2";
        public const string large3 = "BigShark3";

        public const string lev1 = "Stormcaller";
        public const string lev2 = "Shelltooth";
        public const string lev3 = "Scorchclaw";
        public const string lev4 = "Gracefallen";
    }

    //Add whatever Recipe Prefab names here to have them be unlocked at the start of the game
    public struct StartingRecipeUnlockedArray
    {
        public static string[] startUnlockedArray  = new string[]{
        "Common Lure",
        "Sushi Roll",
        "Snack",
        "Bug Wrap"
        };
    }

    public struct SettingValues
    {
        public static gameDifficulty GameDifficulty = gameDifficulty.Unknown;
        public static float playerSensitivity = 1;
        public static float boatCamSensitivity = 1;
        public static float adsCamSensitivity = 1;
        public const float sfxLevel = 0.5f;
        public const float musicLevel = 0.5f;
        public const string sfxSourceTag = "sfxSource";
        public const string musicSourceTag = "musicSource";
        static Vector2[] ScreenRes = new Vector2[8] {
            new Vector2(1280,1024),
            new Vector2(1360,768),
            new Vector2(1366,768),
            new Vector2(1440,900),
            new Vector2(1600,900),
            new Vector2(1680,1050),
            new Vector2(1920,1080),
            new Vector2(2560,1440)
        };
        public static Vector2[] getSupportedRes { get { return ScreenRes; } }

        public const int RENDER_DISTANCE = 8000;
    }

    public struct InternalValues
    {
        public const string gameScene = "Small Fishing Playtest";
        public const float musicFadeTransitionTime = 7f;
        public const float fullVolume = 1f;
        public const string welcomeMessage = "Kaiju Fishing Pre-Alpha 0.0.4.2 \n Gameplay subject to change";
    }

    public struct GameplayValues
    {
        public const float smallSpeedMin = 1.0f;
        public const float smallSpeedMax = 20f;
        public const float mediumSpeedMin = 5f;
        public const float mediumSpeedMax = 35f;
        public const float largeSpeedMin = 8f;
        public const float largeSpeedMax = 50f;
        public const float leviathanSpeedMin = 40f;
        public const float leviathanSpeedMax = 75f;

        public const float smallWeightMin = 2.5f;
        public const float smallWeightMax = 3.5f;
        public const float mediumWeightMin = 2000f;
        public const float mediumWeightMax = 2000f;
        public const float largeWeightMin = 20f;
        public const float largWeightMax = 50f;
        public const float leviathanWeightMin = 100f;
        public const float leviathanWeightMax = 500f;

        public const float fishSpeedMin = 1.0f;
        public const float fishSpeedMax = 51f;
        public const float fishWeightMin = 1.0f;
        public const float fishWeightMax = 501f;

        public const float difficultyMultiplierEasy = 0.8f;
        public const float difficultyMultiplierMedium = 1.0f;
        public const float difficultyMultiplierHard = 1.2f;
        public const float difficultyprivateMaster = 1.4f;

        public const int keepBonusEasy = 10;
        public const int keepBonusMedium = 20;
        public const int keepBonusHard = 50;
        public const int keepBonusMaster = 100;

        /// Used for determine which rarity of fish to spawn
        /// Rolls between 1-100 and checks if value is less than legendary to uncommon. Anything else is Common
        /// So this should total 100
        public const int COMMON_FISH_SPAWN_PERCENT = 62;
        public const int UNCOMMON_FISH_SPAWN_PERCENT = 20;
        public const int RARE_FISH_SPAWN_PERCENT = 10;
        public const int EPIC_FISH_SPAWN_PERCENT = 6;
        public const int LEGENDARY_FISH_SPAWN_PERCENT = 2;
        
        [Obsolete("Fish pull can never be trash because all fish are spawned by name using fish spawn regions now")]
        public const int trashFishSpawnPercent = 10;

        //Equals 1000
        public const int commonLootPercent = 580;
        public const int uncommonLootPercent = 200;
        public const int trashLootPercent = 100;
        public const int rareLootPercent = 80;
        public const int epicLootPercent = 30;
        public const int legendaryLootPercent = 10;

        public const int commonLootShellBaseAmount = 1;
        public const int uncommonLootShellBaseAmount = 3;
        public const int rareLootShellBaseAmount = 5;
        public const int epicLootShellBaseAmount = 8;
        public const int legendaryLootShellBaseAmount = 10;

        public const int commonAttachmentShellBaseAmount = 1;
        public const int uncommonAttachmentShellBaseAmount = 3;
        public const int rareAttachmentShellBaseAmount = 5;
        public const int epicAttachmentShellBaseAmount = 8;
        public const int legendaryAttachmentShellBaseAmount = 10;

        public const int commonLootBaitBaseAmount = 1;
        public const int uncommonLootBaitBaseAmount = 2;
        public const int rareLootBaitBaseAmount = 3;
        public const int epicLootBaitBaseAmount = 4;
        public const int legendaryLootBaitBaseAmount = 5;

        public const int commonLootShellAdjusterVal = 1; //+/- this value
        public const int uncommonLootShellAdjusterVal = 2;
        public const int rareLootShellAdjusterVal = 3;
        public const int epicLootShellAdjusterVal = 4;
        public const int legendaryLootShellAdjusterVal = 5;

        public const int commonLootBaitAdjusterVal = 1; //+/- this value
        public const int uncommonLootBaitAdjusterVal = 2;
        public const int rareLootBaitAdjusterVal = 3;
        public const int epicLootBaitAdjusterVal = 4;
        public const int legendaryLootBaitAdjusterVal = 5;

        public const float pickupRange = 50f;
        public const float respawnResourceRange = 100f;
        public const float featherHeightCheckTick = 0.2f;
        public const float backoutTimer = .5f;
        public const float holdToCraftTime = 0.5f;
        public const int doubleResourceCost = 2;
        public const float uiDelaySeconds = 1;
        public const int startingResourceCount = 10;
        public const int resourceCollectionMin = 1;
        public const int resourceCollectionMax = 6;
        
        //Fishing values
        public const float MAX_TENSION = 97f;

        //units per second
        public const float BASE_FREE_REEL_TENSION_GAIN_RATE = 5f;
        public const float BASE_FIGHT_REEL_TENSION_GAIN_RATE = 50f;
        public const float BASE_FREE_REEL_TENSION_DECREASE_RATE = 50f;
        public const float BASE_FIGHT_REEL_TENSION_DECREASE_RATE = 10f;

        public const float MAX_LINE_LENGTH_DISTANCE_BEFORE_BREAK = 150f;
        
        public const float MAX_REEL_VELOCITY = 10f;
        public const float TIME_TO_FULL_REEL_SPEED = 0.2f;

        //Medium Fishing
        //m/s
        public const float BASE_MEDIUM_REELING_SPEED = 5f;
        public const float BASE_MEDIUM_REELING_TENSION_GAIN_RATE = 35f;
        public const float BASE_MEDIUM_REELING_TENSION_DECREASE_RATE = 30f;
        
        //Scoring values
        public const float MAX_TENSION_IN_RANGE = 80f;
        public const float GREAT_CATCH_TIMING_SMALL = 20f;
        public const float OKAY_CATCH_TIMING_SMALL = 40f;
        public const float BAD_CATCH_TIMING_SMALL = 60f;

        public const int maxHeartContainers = 10;

        public const string recipeLockedDisplayMessage = "Unlockable in the full release";

        public const int inventoryLimitTier1 = 24;
        //not using tiers2&3 for now
        public const int inventoryLimitTier2 = 30;
        public const int inventoryLimitTier3 = 30;

        public const int fishTankSizeLimit1 = 10;
        public const int fishTankSizeLimit2 = 20;
        public const int fishTankSizeLimit3 = 30;
    }
    
    public struct ScoringValues
    {
        public enum Grades {SS,S,A,B,C,D,F}

        public const int QTE_POINTS = 100;
        public const int TIME_MAX_POINTS = 1500;
        public const int INRANGE_MAX_POINTS = 500;
        
        public const int SMALL_D_POINTS = 0;
        public const int SMALL_C_POINTS = 600;
        public const int SMALL_B_POINTS = 1000;
        public const int SMALL_A_POINTS = 1500;
        public const int SMALL_S_POINTS = 2000;
        public const int SMALL_SS_POINTS = 2300;
        
        public const int MEDIUM_D_POINTS = 0;
        public const int MEDIUM_C_POINTS = 0;
        public const int MEDIUM_B_POINTS = 0;
        public const int MEDIUM_A_POINTS = 0;
        public const int MEDIUM_S_POINTS = 0;
        public const int MEDIUM_SS_POINTS = 0;
        
        public const int LARGE_D_POINTS = 0;
        public const int LARGE_C_POINTS = 0;
        public const int LARGE_B_POINTS = 0;
        public const int LARGE_A_POINTS = 0;
        public const int LARGE_S_POINTS = 0;
        public const int LARGE_SS_POINTS = 0;
        
        public const int STORMCALLER_D_POINTS = 0;
        public const int STORMCALLER_C_POINTS = 0;
        public const int STORMCALLER_B_POINTS = 0;
        public const int STORMCALLER_A_POINTS = 0;
        public const int STORMCALLER_S_POINTS = 0;
        public const int STORMCALLER_SS_POINTS = 0;
    }

    public struct FishAIValues
    {
        public const float TIME_TO_FULL_SPEED = 0.5f;
        public const float FISH_SPEED_MIN = 6f;
        public const float FISH_SPEED_MAX = 10f;
        public const float FISH_REST_DURATION_MIN = 0.5f;
        public const float FISH_REST_DURATION_MAX = 4f;
        public const float FISH_FIGHT_DURATION_MIN = 0.5f;
        public const float FISH_FIGHT_DURATION_MAX = 4f;

        public const float BASE_MEDIUM_TOP_SPEED = 20f;
        public const float BASE_MEDIUM_ACCELERATION = 20f;
    }

    public static readonly string[] SUCCESSFUL_CATCH_PHRASES =
    {
        "You did Amazing!",
        "Hook, Line, and Sinker!",
        "Top-notch Work!",
        "That's a Big One!",
        "Reel Nice!",
        "You, sir, are a fish.",
        "Nice Fish Bro.",
        "Cute Catch!",
        "What a Catch!",
        "Awesome Catch!",
        "Nothin but Net!",
        "Catchtacular!",
        "You Hooked a Big One!",
        "That's a Keeper for Sure!",
        "Fish On!",
        "Well done Angler!",
        "You Landed that Perfectly",
        "Fisherman's Luck!",
        "Caught with Style!",
        "Patience Pays Off!",
        "Catch of the Day!",
        "Reeled like a Pro!"
    };

    public static readonly string[] COMBO_CATCH_PHRASES =
    {
        "Double Catch!",
        "Triple Catch!",
        "Quadra Catch!",
        "Penta Catch!",
        "Mega Catch!",
        "Monster Catch!",
        "Unstoppable Catch!",
        "Catch-pocalypse!",
        "Godlike Catch!"
    };

    public struct QTESuccessValues
    {
        public const float tensionReduction = 3f;
        //IDEAS
        //Bonus to fishing rod maneuverability
        //Reeling Power (faster reeling) - Rarer effect
        //Slow fish movement and possibly stunning - Useful against tough fish to catch
    }

    public struct TadfishSchoolValues
    {
        public const float cohesionRadius = 30f;
        public const float cohesionWeight = 30f;
        public const float alignmentWeight = 1000f;
        public const float separationRadius = 20f;
        public const float separationWeight = 5000f;
        public const float maxAcceleration = 20f; 
        public const float disappearDepth = -50f;
        public const float schoolRadius = 50f;
    }

    public struct BaitResourceModifiers
    {
        //Bait Advantage Modifiers
        //Plant
        public const float tensionReductionMultiplier = 0.8f; //80% the tension increases by 80% of what it should (20% reduced)
        //Bug
        public const float fishActionDelayMultiplier = 1.5f; //The time between fish actions is multiplied by this if it was 2s (2s + 1.5 = 3s)
        //Meat
        public const float nibbleReductionMultiplier = 0.6f; //Multiplied by the amount of nibbles the fish would rounded up. Ex. 3 = (3 * 0.5 = 1.8 = 2) 0.5 is rounded down

    }

    public struct OptimizationValues
    {
        //ZACH Added 7/16/21
        //The distance the camera has to be in order for a fish region or portal to spawn fish.
        //This helps to limit lag on startup and have it generate as the player explores the world.
        public const float distCloseToCameraToSpawnFish = 600f;

        public const float distToCameraForFishVisibility = 500f;
        public const float optimizationTick = 0.25f;
    }

    public struct FishRegionValues
    {
        public const int wanderingAIPathResolution = 15;
        public const int wanderingAITravelRadius = 40;
        public const int wanderingAIPathLength = 10;
        public const int wanderingAIRandomness = 3;
    }

    public struct SteamWorksData
    {
        public const int appId = 1043110;
    }

    public static bool IsNull<T>(this T myObject, string message = "") where T : class
    {
        switch (myObject)
        {
            case UnityEngine.Object obj when !obj:
                Debug.Log("The object is null! " + message);
                return true;
            case null:
                Debug.Log("The object is null! " + message);
                return true;
            default:
                return false;
        }
    }

    /*
    private static Common _instance;
    public static Common Instance { get { return _instance; } }

    void Awake()
    {
        //Setup Singleton
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }
    */
}
