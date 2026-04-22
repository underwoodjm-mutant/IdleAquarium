using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Crest;
//using Language.Lua;
//using System.IO.Ports;
//using ES3Types;
//using ES3Internal;

[SerializeField]
public enum FishType { Small, Medium, Large, Leviathan, Error }
[SerializeField]
public enum Habitat { Normal, Volcanic, Arctic, Tropical, Error }
[SerializeField]
public enum Depth { Shallow, Medium, Deep, Error }
[SerializeField]
public enum Attractor { Omnivore, Carnivore, Vegetarian, Error }
[SerializeField]
public enum Rarity { Common, Uncommon, Trash, Rare, Epic, Legendary, Error }
[SerializeField]
public enum FishState { Idle, PreHook, ReelDelay, Swim, Pulled, Action, Hooked, Caught, Loot, Free, Error }
[SerializeField]
public enum Difficulty { Easy, Medium, Hard, Master, Error }

public class FishBase : MonoBehaviour
{
    //public GameObject fishPrefab;

    [SerializeField]
    FishType _myType = FishType.Small;
    [SerializeField]
    FishState _myState = FishState.Idle;
    [SerializeField]
    private int _ID;
    //FishName is what we're using to for subtype of the fish type
    //so Tadfish, Flamander, PuffPuff for the small fish
    //See PlayerQuestController.CaughtAFishCheckForQuest() for details
    //quest is the only thing that cares about subtypes (for now)
    [SerializeField]
    private string _myName;
    [SerializeField]
    private string _description;
    [SerializeField]
    private Habitat _habitat;
    //_catchTier added 12/13/24 JU
    //Depth was not being used - we added a catching tier system to enhance skill based catching
    [SerializeField]
    private int _catchTier;
    [SerializeField]
    private Attractor _attractor;
    [SerializeField]
    private Rarity _rarity;
    [SerializeField]
    private int _specialLootID;
    [SerializeField]
    private Difficulty _difficulty;
    [SerializeField]
    private float fishWeight;
    [SerializeField]
    //In regards to the line tension
    //Depends on the weight class of the fish. Not sure where to determine this. Probably in manager.
    //Small: 2, Medium: 1.8, Large: 1.6, Leviathan: 1.4
    private float _weightClassMult;
    [SerializeField]
    private float fishSpeed;
    [SerializeField]
    private float fishStamina;
    [SerializeField]
    private bool _discovered;
    [SerializeField]
    private int _boonBonus;
    [SerializeField]
    private int _fishExperience;
    [SerializeField]
    private string _movementDescription;//added 7/19/22 JU - for KaijuDexJournal
    [SerializeField]
    private int _catchMissionCount1;
    [SerializeField]
    private int _catchMissionCount2;
    [SerializeField]
    private int _catchMissionCount3;//added 7/19/22 JU - for KaijuDexJournal

    [Header("Inventory Variabels")]
    public Sprite Icon, BG;


    /// <summary>
    /// //These are all public for Gameplay Designer adjustment
    /// TODO - CHANGE TO PRIVATE AFTER GD PLAYTESTING PASS
    /// </summary>
    public float smallSpeedMin = 3f;
    public float smallSpeedMax = 6f;
    public float mediumSpeedMin = 8f;
    public float mediumSpeedMax = 13f;
    public float largeSpeedMin = 15f;
    public float largeSpeedMax = 25f;
    public float leviathanSpeedMin = 40f;
    public float leviathanSpeedMax = 75f;

    public float smallWeightMin = 1.0f;
    public float smallWeightMax = 5f;
    //public float mediumWeightMin = 6f;
    public float mediumWeightMin = 5f;
    //public float mediumWeightMax = 20f;
    public float mediumWeightMax = 15f;
    public float largeWeightMin = 20f;
    public float largeWeightMax = 50f;
    public float leviathanWeightMin = 100f;
    public float leviathanWeightMax = 500f;

    public float fishSpeedMin = 1.0f;
    public float fishSpeedMax = 51f;
    public float fishWeightMin = 1.0f;
    public float fishWeightMax = 501f;

    public float difficultyMultiplierEasy = 0.8f;
    public float difficultyMultiplierMedium = 1.0f;
    public float difficultyMultiplierHard = 1.2f;
    public float difficultyprivateMaster = 1.4f;

    public FishType FishType { get { return _myType; } set { _myType = value; } }
    public int FishID { get { return _ID; } set { _ID = value; } }
    public string FishName { get { return _myName; } set { _myName = value; } }
    public string FishDescription { get { return _description; } set { _description = value; } }
    public Habitat FishHabitat { get { return _habitat; } set { _habitat = value; } }
    public int CatchTier { get { return _catchTier; } set { _catchTier = value; } }
    public Attractor FishAttractor { get {return _attractor; } set { _attractor = value; } }
    public Rarity FishRarity { get { return _rarity; } set { _rarity = value; } }
    public int SpecialLootID { get {return _specialLootID; } set {_specialLootID = value; } }
    public float FishWeight { get { return fishWeight; } set { fishWeight = value; } }
    public float FishSpeed { get {return fishSpeed; } set {fishSpeed = value; } }
    public FishState MyState { get { return _myState; } set { _myState = value; } }
    public float WeightClassMultiplyer { get { return _weightClassMult; } set { _weightClassMult = value; } }
    public Difficulty Difficulty { get { return _difficulty; } set { _difficulty = value; } }
    public bool Discovered { get { return _discovered; } set { _discovered = value; } }
    public int NumberCaught { get { return _boonBonus; } set { _boonBonus = value; } }
    public float FishStamina { get { return fishStamina; } set { fishStamina = value; } }
    public int FishExperience { get { return _fishExperience; } set { _fishExperience = value; } }
    public string FishMovementDescription { get { return _movementDescription; } set { _movementDescription = value; } }
    public int CatchMissionCount1 { get { return _catchMissionCount1; } set { _catchMissionCount1 = value; } }
    public int CatchMissionCount2 { get { return _catchMissionCount2; } set { _catchMissionCount2 = value; } }
    public int CatchMissionCount3 { get { return _catchMissionCount3; } set { _catchMissionCount3 = value; } }
    public int BoonBonus { get { return _boonBonus; } set { _boonBonus = value; } }

    //Empty Constructor
    public FishBase()
    {

    }

    public FishBase(FishBase newFish){
        FishType = newFish.FishType;
        FishID = newFish.FishID;
        FishName = newFish.FishName;
        FishDescription = newFish.FishDescription;
        FishHabitat = newFish.FishHabitat;
        CatchTier = newFish.CatchTier;
        FishAttractor = newFish.FishAttractor;
        FishRarity = newFish.FishRarity;
        SpecialLootID = newFish.SpecialLootID;
        MyState = newFish.MyState;
        Difficulty = newFish.Difficulty;
        //GenerateFishClassMultByFishType(type);
        WeightClassMultiplyer = newFish.WeightClassMultiplyer;
        FishWeight = 15f;
        FishSpeed = 5f;
        FishExperience = newFish.FishExperience;
        FishMovementDescription = newFish.FishMovementDescription;
        CatchMissionCount1 = newFish.CatchMissionCount1;
        CatchMissionCount2 = newFish.CatchMissionCount2;
        CatchMissionCount3 = newFish.CatchMissionCount3;
        BoonBonus = newFish.BoonBonus;
    }

    //Constructor
    public FishBase(FishType type, int id, string name, string description, Habitat habitat, int catchTier, Attractor attractor, Rarity rarity, int special, FishState fishState, Difficulty difficulty, float wcm, int exp, string movement, int catchcount1, int catchcount2, int catchcount3, int boonBonus)
    {
        FishType = type;
        FishID = id;
        FishName = name;
        FishDescription = description;
        FishHabitat = habitat;
        CatchTier = catchTier;
        FishAttractor = attractor;
        FishRarity = rarity;
        SpecialLootID = special;
        MyState = fishState;
        Difficulty = difficulty;
        //GenerateFishClassMultByFishType(type);
        WeightClassMultiplyer = wcm;
        FishWeight = 15f;
        FishSpeed = 5f;
        FishExperience = exp;
        FishMovementDescription = movement;
        CatchMissionCount1 = catchcount1;
        CatchMissionCount2 = catchcount2;
        CatchMissionCount3 = catchcount3;
        BoonBonus = boonBonus;
    }

    public float GetPercentageOfMaxWeight()
    {
        float maxWeight = 0;
        float weightPercentage;

        switch (FishType)
        {
            case FishType.Small:
                maxWeight = smallWeightMax;
                break;
            case FishType.Medium:
                maxWeight = mediumWeightMax;
                break;
            case FishType.Large:
                maxWeight = largeWeightMax;
                break;
            case FishType.Leviathan:
                maxWeight = leviathanSpeedMax;
                break;
            case FishType.Error:
                Debug.LogError("Fish Error");
                break;
            default:
                break;
        }

        if (maxWeight == 0)
        {
            weightPercentage = 0;
            Debug.LogError("Issue with Fishtype");
        }
        else
        {
            weightPercentage = fishWeight / maxWeight;
        }
        return weightPercentage;
    }

    private float GenerateRandomWeightByFishType(FishType fishtype, Difficulty difficulty)
    {
        float temp = 0f;
        float difficultyModifier = 1f;
        switch (difficulty)
        {
            case Difficulty.Easy:
                difficultyModifier = 0.8f;
                break;
            case Difficulty.Medium:
                difficultyModifier = 1.0f;
                break;
            case Difficulty.Hard:
                difficultyModifier = 1.2f;
                break;
            case Difficulty.Master:
                difficultyModifier = 1.4f;
                break;
            case Difficulty.Error:
                difficultyModifier = 1.0f;
                Debug.LogError("Error in FishBase.GenerateRandomWeightByFishType on difficulty");
                break;
            default:
                difficultyModifier = 1.0f;
                break;
        }

        switch (fishtype)
        {
            case FishType.Small:
                temp = difficultyModifier*Random.Range(smallWeightMin, smallWeightMax);
                break;
            case FishType.Medium:
                temp = difficultyModifier * Random.Range(mediumWeightMin, mediumWeightMax);
                break;
            case FishType.Large:
                temp = difficultyModifier * Random.Range(largeWeightMin, largeWeightMax);
                break;
            case FishType.Leviathan:
                temp = difficultyModifier * Random.Range(leviathanWeightMin, leviathanWeightMax);
                break;
            case FishType.Error:
                Debug.LogError("Error in FishBase.GenerateRandomWeightByFishType on fishtype");
                break;
            default:
                break;
        }
        return temp;
    }

    private float GenerateRandomSpeedByFishType(FishType fishtype, Difficulty difficulty)
    {
        float temp = 0f;
        float difficultyModifier = 1f;
        switch (difficulty)
        {
            case Difficulty.Easy:
                difficultyModifier = 0.8f;
                break;
            case Difficulty.Medium:
                difficultyModifier = 1.0f;
                break;
            case Difficulty.Hard:
                difficultyModifier = 1.2f;
                break;
            case Difficulty.Master:
                difficultyModifier = 1.4f;
                break;
            case Difficulty.Error:
                difficultyModifier = 1.0f;
                Debug.LogError("Error in FishBase.GenerateRandomWeightByFishType on difficulty");
                break;
            default:
                difficultyModifier = 1.0f;
                break;
        }
        switch (fishtype)
        {
            case FishType.Small:
                temp = difficultyModifier*Random.Range(smallSpeedMin, smallSpeedMax);
                break;
            case FishType.Medium:
                temp = difficultyModifier * Random.Range(mediumSpeedMin, mediumSpeedMax);
                break;
            case FishType.Large:
                temp = difficultyModifier * Random.Range(largeSpeedMin, largeSpeedMax);
                break;
            case FishType.Leviathan:
                temp = difficultyModifier * Random.Range(leviathanSpeedMin, leviathanSpeedMax);
                break;
            case FishType.Error:
                Debug.LogError("Error in FishBase.GenerateRandomWeightByFishType");
                break;
            default:
                break;
        }
        return temp;
    }

    public void SetFishByHabitatDepth(FishType fishtype, int id, string name, string description, Habitat habitat, int catchTier, Attractor attractor, Rarity rarity, int spid, FishState state, Difficulty difficulty, float wcm, int exp)
    {
        FishType = fishtype;
        FishID = id;
        FishName = name;
        FishDescription = description;
        FishHabitat = habitat;
        CatchTier = catchTier;
        FishAttractor = attractor;
        FishRarity = rarity;
        SpecialLootID = spid;
        FishWeight = GenerateRandomWeightByFishType(fishtype, difficulty);
        FishStamina = FishWeight + fishStamina;
        FishSpeed = GenerateRandomSpeedByFishType(fishtype, difficulty);
        MyState = state;
        Difficulty = difficulty;
        WeightClassMultiplyer = wcm;
        FishExperience = exp;
    }

    public void SetSmallTestFish()
    {
        FishType = FishType.Small;
        FishID = 1;
        FishName = "Test";
        FishDescription = "Testing";
        FishHabitat = Habitat.Normal;
        //FishDepth = Depth.Medium;
        CatchTier = (int)Common.ScoringValues.Grades.F;
        FishAttractor = Attractor.Omnivore;
        FishRarity = Rarity.Common;
        SpecialLootID = 0;
        FishWeight = 3f;
        FishSpeed = 5f;
        MyState = FishState.Idle;
        Difficulty = Difficulty.Medium;
    }

    public void SetMediumTestFish()
    {
        FishType = FishType.Medium;
        FishID = 1;
        FishName = "Test";
        FishDescription = "Testing";
        FishHabitat = Habitat.Normal;
        //FishDepth = Depth.Medium;
        CatchTier = (int)Common.ScoringValues.Grades.F;
        FishAttractor = Attractor.Omnivore;
        FishRarity = Rarity.Common;
        SpecialLootID = 0;
        FishWeight = 15f;
        FishSpeed = 5f;
        MyState = FishState.Idle;
        Difficulty = Difficulty.Medium;
    }

    public void SetAllRandom()
    {
        FishType = FishType.Medium;
        FishID = 1;
        FishName = "Test";
        FishDescription = "Testing";
        FishHabitat = Habitat.Normal;
        //FishDepth = Depth.Medium;
        CatchTier = (int)Common.ScoringValues.Grades.F;
        FishAttractor = Attractor.Omnivore;
        FishRarity = Rarity.Common;
        SpecialLootID = 0;
        FishWeight = 20f;
        FishSpeed = 5f;
        MyState = FishState.Idle;
        Difficulty = Difficulty.Medium;
    }

    public void GenerateNormalSmallShallowFish()
    {
        FishType = FishType.Small;
        FishID = 1;
        FishName = "Test";
        FishDescription = "Testing";
        FishHabitat = Habitat.Normal;
        //FishDepth = Depth.Medium;
        CatchTier = (int)Common.ScoringValues.Grades.F;
        FishAttractor = Attractor.Omnivore;
        FishRarity = Rarity.Common;
        SpecialLootID = 0;
        FishWeight = 3f;
        FishSpeed = 5f;
        MyState = FishState.Idle;
        Difficulty = Difficulty.Medium;
    }

    public int GetValueBasedOnRarity(int commonReturn, int uncommonReturn, int rareReturn, int epicReturn, int legendaryReturn, int trashReturn, int errorReturn, int defaultReturn)
    {
        Rarity myRarity = this.FishRarity;

        switch (myRarity)
        {
            case Rarity.Common:
                return commonReturn;

            case Rarity.Uncommon:
                return uncommonReturn;

            case Rarity.Trash:
                return trashReturn;

            case Rarity.Rare:
                return rareReturn;

            case Rarity.Epic:
                return epicReturn;

            case Rarity.Legendary:
                return legendaryReturn;

            case Rarity.Error:
                return errorReturn;

            default:
                return defaultReturn;

        }
    }
}
