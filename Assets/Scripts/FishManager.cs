using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Refactored FishManager:
/// - Inherits from generic Singleton<T>.
/// - Prefer data lookups via FishDatabase (if available) and instantiation via FishSpawner.
/// - Keeps legacy lists for compatibility but marks them obsolete and provides new APIs.
/// - Consolidates spawn-table generation and prefab instantiation.
/// </summary>
public class FishManager : Singleton<FishManager>
{
    // Alpha / GD toggles
    public bool allowSmall;
    public bool allowMedium;
    public bool allowLarge;
    public bool allowLeviathan;

    // Prefab sources
    [Tooltip("Legacy prefab list (1-based ordering expected). Prefer using FishSpawner.")]
    public List<GameObject> fishPrefabList;
    public GameObject errorFishPrefab;
    public FishSpawner fishSpawner; // optional, preferred for robust id->prefab mapping

    // Spawned reference (legacy compatibility)
    public GameObject spawnedFish;

    // Prefer using FishDatabase for data-driven queries
    private FishDatabase _fishDatabase;

    // Data containers (legacy types preserved for compatibility)
    private FishBase _tempFish;
    public List<FishBase> allFish;

    [Obsolete("Use FishDatabase queries (GetFishDataByHabitat / GetFishDataById) instead.")]
    public List<FishBase> habitatNormalFishBaseList;
    [Obsolete("Use FishDatabase queries (GetFishDataByHabitat / GetFishDataById) instead.")]
    public List<FishBase> habitatVolcanicFishBaseList;
    [Obsolete("Use FishDatabase queries (GetFishDataByHabitat / GetFishDataById) instead.")]
    public List<FishBase> habitatArcticFishBaseList;
    [Obsolete("Use FishDatabase queries (GetFishDataByHabitat / GetFishDataById) instead.")]
    public List<FishBase> habitatTropicalFishBaseList;

    // Spawn tables (legacy public fields preserved but generated from DB or CSV)
    [Obsolete("Spawn tables are generated internally. Prefer Spawn APIs that use FishDatabase/FishSpawner.")]
    public List<FishBase> normalShallowFishTable;
    [Obsolete("Spawn tables are generated internally. Prefer Spawn APIs that use FishDatabase/FishSpawner.")]
    public List<FishBase> normalMediumFishTable;
    [Obsolete("Spawn tables are generated internally. Prefer Spawn APIs that use FishDatabase/FishSpawner.")]
    public List<FishBase> normalDeepFishTable;
    [Obsolete("Spawn tables are generated internally. Prefer Spawn APIs that use FishDatabase/FishSpawner.")]
    public List<FishBase> volcanicShallowFishTable;
    [Obsolete("Spawn tables are generated internally. Prefer Spawn APIs that use FishDatabase/FishSpawner.")]
    public List<FishBase> volcanicMediumFishTable;
    [Obsolete("Spawn tables are generated internally. Prefer Spawn APIs that use FishDatabase/FishSpawner.")]
    public List<FishBase> volcanicDeepFishTable;
    [Obsolete("Spawn tables are generated internally. Prefer Spawn APIs that use FishDatabase/FishSpawner.")]
    public List<FishBase> arcticShallowFishTable;
    [Obsolete("Spawn tables are generated internally. Prefer Spawn APIs that use FishDatabase/FishSpawner.")]
    public List<FishBase> arcticMediumFishTable;
    [Obsolete("Spawn tables are generated internally. Prefer Spawn APIs that use FishDatabase/FishSpawner.")]
    public List<FishBase> arcticDeepFishTable;
    [Obsolete("Spawn tables are generated internally. Prefer Spawn APIs that use FishDatabase/FishSpawner.")]
    public List<FishBase> tropicalShallowFishTable;
    [Obsolete("Spawn tables are generated internally. Prefer Spawn APIs that use FishDatabase/FishSpawner.")]
    public List<FishBase> tropicalMediumFishTable;
    [Obsolete("Spawn tables are generated internally. Prefer Spawn APIs that use FishDatabase/FishSpawner.")]
    public List<FishBase> tropicalDeepFishTable;

    public List<FishBase> fishDiscoveredListBySpecies;

    // Legacy unused fields left for compatibility
    [Obsolete("changed these to Common static global vars (Common.GameplayValues.COMMON_FISH_SPAWN_PERCENT")]
    private int commonPercent = 58;
    [Obsolete("changed these to Common static global vars (Common.GameplayValues.UNCOMMON_FISH_SPAWN_PERCENT")]
    private int uncommonPercent = 20;
    [Obsolete("Not used anymore since fish can't be spawned as trash")]
    private int trashPercent = 10;
    [Obsolete("changed these to Common static global vars (Common.GameplayValues.RARE_FISH_SPAWN_PERCENT")]
    private int rarePercent = 8;
    [Obsolete("changed these to Common static global vars (Common.GameplayValues.EPIC_FISH_SPAWN_PERCENT")]
    private int epicPercent = 3;
    [Obsolete("changed these to Common static global vars (Common.GameplayValues.LEGENDARY_FISH_SPAWN_PERCENT")]
    private int legendaryPercent = 1;

    // Legacy CSV filename kept for compatibility if needed
    public string fishFile = "Fish";

    public override void Awake()
    {
        base.Awake();

        // Initialize containers
        _tempFish = new FishBase();
        allFish = new List<FishBase>();
        fishDiscoveredListBySpecies = new List<FishBase>();

        habitatNormalFishBaseList = new List<FishBase>();
        habitatVolcanicFishBaseList = new List<FishBase>();
        habitatArcticFishBaseList = new List<FishBase>();
        habitatTropicalFishBaseList = new List<FishBase>();

        // Attempt to load FishDatabase from Resources first (preferred).
        // If you store the SO under Resources (e.g. Assets/Resources/Data/FishDatabase.asset),
        // Resource loading will succeed. Otherwise we fallback to legacy CSV processing.
        _fishDatabase = Resources.Load<FishDatabase>("FishDatabase");
        if (_fishDatabase != null && _fishDatabase.fishes != null && _fishDatabase.fishes.Count > 0)
        {
            PopulateFromDatabase(_fishDatabase);
        }
        else
        {
            PopulateFromCsvLegacy();
        }

        // Generate spawn tables (legacy structure preserved for compatibility)
        GenerateAllSpawnTables();
    }

    // PUBLIC NEW API — prefer these in other systems:
    // - Query fish data by id
    // - Query fish data by habitat
    // - Spawn by id via FishSpawner (if configured) or legacy list

    /// <summary>
    /// Returns FishData (authoritative) if FishDatabase is available; otherwise maps legacy FishBase to FishData-like result (via runtime lists).
    /// </summary>
    public FishData GetFishDataById(int id)
    {
        if (_fishDatabase != null)
        {
            return _fishDatabase.GetById(id);
        }

        var legacy = allFish?.Find(f => f.FishID == id);
        if (legacy == null) return null;

        return MapLegacyToData(legacy);
    }

    /// <summary>
    /// Returns a list of FishData for a given habitat (prefers FishDatabase).
    /// </summary>
    public List<FishData> GetFishDataByHabitat(Habitat habitat)
    {
        if (_fishDatabase != null)
        {
            return _fishDatabase.fishes.Where(fd => fd.Habitat == habitat).ToList();
        }

        var list = GetCollectionForCurrentHabitatInternal(habitat);
        return list?.Select(MapLegacyToData).Where(x => x != null).ToList() ?? new List<FishData>();
    }

    /// <summary>
    /// Spawn a fish by id using FishSpawner if available; otherwise falls back to legacy prefab list.
    /// Returns the instantiated GameObject or null.
    /// </summary>
    public GameObject SpawnById(int fishId, Vector3 pos)
    {
        return InstantiateFishById(fishId, pos);
    }

    /// <summary>
    /// Spawn a fish by name using FishDatabase when available; falls back to legacy per-habitat lists.
    /// Uses rarity roll and FishSpawner for instantiation.
    /// </summary>
    public GameObject SpawnByNameUsingDatabase(string name, Vector3 pos)
    {
        List<FishData> candidates;
        if (_fishDatabase != null)
        {
            // find entries across habitats that match name
            candidates = _fishDatabase.fishes.Where(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        else
        {
            var collection = GetCollectionForCurrentHabitat();
            candidates = collection.Select(MapLegacyToData).Where(f => f != null && string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (candidates == null || candidates.Count == 0) return null;

        int roll = UnityEngine.Random.Range(1, 101);
        Rarity chosenRarity = DetermineRarityFromRoll(roll);

        var filtered = candidates.Where(c =>
        {
            // DB uses FishData.Rarity; legacy mapping uses Rarity from FishBase -> Map to FishData.Rarity (same enum)
            return c.Rarity == chosenRarity;
        }).ToList();

        if (filtered.Count == 0)
            filtered = candidates;

        var selected = filtered[UnityEngine.Random.Range(0, filtered.Count)];
        var instance = InstantiateFishById(selected.Id, pos);
        if (instance == null) return null;

        // If legacy systems expect a FishBase MonoBehaviour, attach and initialize minimally.
        var fb = instance.GetComponent<FishBase>() ?? instance.AddComponent<FishBase>();
        fb.SetFishByHabitatDepth(selected.Type, selected.Id, selected.Name, selected.Description, selected.Habitat, selected.CatchTier, selected.Attractor, selected.Rarity, selected.SpecialLootId, FishState.Swim, selected.Difficulty, selected.WeightClassMultiplier, selected.Experience);

        spawnedFish = instance;
        return instance;
    }

    // -------------------------
    // Internal helpers & legacy-preserving methods
    // -------------------------

    private FishData MapLegacyToData(FishBase legacy)
    {
        if (legacy == null) return null;
        return new FishData
        {
            Id = legacy.FishID,
            Name = legacy.FishName,
            Description = legacy.FishDescription,
            Type = legacy.FishType,
            Habitat = legacy.FishHabitat,
            CatchTier = legacy.CatchTier,
            Attractor = legacy.FishAttractor,
            Rarity = legacy.FishRarity,
            SpecialLootId = legacy.SpecialLootID,
            Difficulty = legacy.Difficulty,
            WeightClassMultiplier = legacy.WeightClassMultiplyer,
            Experience = legacy.FishExperience,
            MovementDescription = legacy.FishMovementDescription,
            Weight = legacy.FishWeight,
            Speed = legacy.FishSpeed,
            NumberCaught = legacy.NumberCaught,
            Discovered = legacy.Discovered
        };
    }

    // Populate from ScriptableObject database (keeps legacy FishBase lists for compatibility)
    private void PopulateFromDatabase(FishDatabase db)
    {
        allFish.Clear();

        for (int i = 0; i < db.fishes.Count; i++)
        {
            var d = db.fishes[i];

            var fb = new FishBase(
                d.Type,
                d.Id,
                d.Name,
                d.Description,
                d.Habitat,
                d.CatchTier,
                d.Attractor,
                d.Rarity,
                d.SpecialLootId,
                FishState.Idle,
                d.Difficulty,
                d.WeightClassMultiplier,
                d.Experience,
                d.MovementDescription,
                d.CatchMissionCount1,
                d.CatchMissionCount2,
                d.CatchMissionCount3,
                d.BoonBonus
            );

            allFish.Add(fb);

            if ((i % 5) == 0) // preserve old species grouping behavior
                fishDiscoveredListBySpecies.Add(fb);

            // distribute by habitat (legacy)
            switch (fb.FishHabitat)
            {
                case Habitat.Normal:
                    habitatNormalFishBaseList.Add(fb);
                    break;
                case Habitat.Volcanic:
                    habitatVolcanicFishBaseList.Add(fb);
                    break;
                case Habitat.Arctic:
                    habitatArcticFishBaseList.Add(fb);
                    break;
                case Habitat.Tropical:
                    habitatTropicalFishBaseList.Add(fb);
                    break;
                default:
                    Debug.LogWarning($"FishManager: Unknown habitat for fish id {fb.FishID}");
                    break;
            }
        }
    }

    // Legacy CSV loading preserved
    private void PopulateFromCsvLegacy()
    {
        List<Dictionary<string, object>> data;
        try
        {
            data = CSVReader.Read(fishFile);
        }
        catch (Exception ex)
        {
            Debug.LogError($"FishManager: Failed to read CSV '{fishFile}': {ex}");
            return;
        }

        FishType fType;
        Habitat fHab;
        Attractor fAtt;
        Rarity fRar;
        Difficulty fDiff;

        for (int i = 0; i < data.Count; i++)
        {
            var row = data[i];
            Enum.TryParse((string)row["FishType"], true, out fType);
            Enum.TryParse((string)row["Habitat"], true, out fHab);
            Enum.TryParse((string)row["Attractor"], true, out fAtt);
            Enum.TryParse((string)row["Rarity"], true, out fRar);
            Enum.TryParse((string)row["Difficulty"], true, out fDiff);

            var fb = new FishBase(
                fType,
                (int)row["ID"],
                (string)row["Name"],
                (string)row["Description"],
                fHab,
                (int)row["CatchTier"],
                fAtt,
                fRar,
                (int)row["SpecialLootID"],
                FishState.Idle,
                fDiff,
                GenerateFishClassMultByFishType(fType),
                (int)row["Experience"],
                (string)row["Movement"],
                (int)row["CatchMissionCount1"],
                (int)row["CatchMissionCount2"],
                (int)row["CatchMissionCount3"],
                (int)row["BoonBonus"]
            );

            allFish.Add(fb);

            if ((i % 5) == 0)
                fishDiscoveredListBySpecies.Add(fb);

            switch (fb.FishHabitat)
            {
                case Habitat.Normal:
                    habitatNormalFishBaseList.Add(fb);
                    break;
                case Habitat.Volcanic:
                    habitatVolcanicFishBaseList.Add(fb);
                    break;
                case Habitat.Arctic:
                    habitatArcticFishBaseList.Add(fb);
                    break;
                case Habitat.Tropical:
                    habitatTropicalFishBaseList.Add(fb);
                    break;
                default:
                    Debug.LogWarning($"FishManager: Unknown habitat for fish id {fb.FishID}");
                    break;
            }
        }
    }

    // Consolidated spawn table generation (legacy)
    private void GenerateAllSpawnTables()
    {
        normalShallowFishTable = BuildSpawnTable(habitatNormalFishBaseList);
        normalMediumFishTable = BuildSpawnTable(habitatNormalFishBaseList);
        normalDeepFishTable = BuildSpawnTable(habitatNormalFishBaseList);

        volcanicShallowFishTable = BuildSpawnTable(habitatVolcanicFishBaseList);
        volcanicMediumFishTable = BuildSpawnTable(habitatVolcanicFishBaseList);
        volcanicDeepFishTable = BuildSpawnTable(habitatVolcanicFishBaseList);

        arcticShallowFishTable = BuildSpawnTable(habitatArcticFishBaseList);
        arcticMediumFishTable = BuildSpawnTable(habitatArcticFishBaseList);
        arcticDeepFishTable = BuildSpawnTable(habitatArcticFishBaseList);

        tropicalShallowFishTable = BuildSpawnTable(habitatTropicalFishBaseList);
        tropicalMediumFishTable = BuildSpawnTable(habitatTropicalFishBaseList);
        tropicalDeepFishTable = BuildSpawnTable(habitatTropicalFishBaseList);
    }

    private List<FishBase> BuildSpawnTable(List<FishBase> source)
    {
        var table = new List<FishBase>();
        if (source == null || source.Count == 0) return table;

        var byRarity = new Dictionary<Rarity, List<FishBase>>();
        foreach (Rarity r in Enum.GetValues(typeof(Rarity)))
            byRarity[r] = new List<FishBase>();

        foreach (var f in source)
        {
            if (byRarity.ContainsKey(f.FishRarity))
                byRarity[f.FishRarity].Add(f);
        }

        void AddRepeated(List<FishBase> list, int percent)
        {
            if (list == null || list.Count == 0 || percent <= 0) return;
            int i = 0;
            while (i < percent)
            {
                table.Add(list[i % list.Count]);
                i++;
            }
        }

        AddRepeated(byRarity[Rarity.Common], Common.GameplayValues.COMMON_FISH_SPAWN_PERCENT);
        AddRepeated(byRarity[Rarity.Uncommon], Common.GameplayValues.UNCOMMON_FISH_SPAWN_PERCENT);
        AddRepeated(byRarity[Rarity.Trash], Common.GameplayValues.trashFishSpawnPercent);
        AddRepeated(byRarity[Rarity.Rare], Common.GameplayValues.RARE_FISH_SPAWN_PERCENT);
        AddRepeated(byRarity[Rarity.Epic], Common.GameplayValues.EPIC_FISH_SPAWN_PERCENT);
        AddRepeated(byRarity[Rarity.Legendary], Common.GameplayValues.LEGENDARY_FISH_SPAWN_PERCENT);

        return table;
    }

    public float GenerateFishClassMultByFishType(FishType type)
    {
        return type switch
        {
            FishType.Small => 2f,
            FishType.Medium => 1.8f,
            FishType.Large => 1.6f,
            FishType.Leviathan => 1.4f,
            _ => 2f,
        };
    }

    public FishBase GetRandomFish()
    {
        if (allFish == null || allFish.Count == 0) return null;
        return allFish[UnityEngine.Random.Range(0, allFish.Count)];
    }

    private List<FishBase> GetSpawnTable(Habitat habitat, Depth depth)
    {
        switch (habitat)
        {
            case Habitat.Normal:
                return depth switch
                {
                    Depth.Shallow => normalShallowFishTable,
                    Depth.Medium => normalMediumFishTable,
                    Depth.Deep => normalDeepFishTable,
                    _ => normalMediumFishTable
                };
            case Habitat.Volcanic:
                return depth switch
                {
                    Depth.Shallow => volcanicShallowFishTable,
                    Depth.Medium => volcanicMediumFishTable,
                    Depth.Deep => volcanicDeepFishTable,
                    _ => volcanicMediumFishTable
                };
            case Habitat.Arctic:
                return depth switch
                {
                    Depth.Shallow => arcticShallowFishTable,
                    Depth.Medium => arcticMediumFishTable,
                    Depth.Deep => arcticDeepFishTable,
                    _ => arcticMediumFishTable
                };
            case Habitat.Tropical:
                return depth switch
                {
                    Depth.Shallow => tropicalShallowFishTable,
                    Depth.Medium => tropicalMediumFishTable,
                    Depth.Deep => tropicalDeepFishTable,
                    _ => tropicalMediumFishTable
                };
            default:
                return normalMediumFishTable;
        }
    }

    private FishBase GetFishByHabitatDepth(Habitat habitat, Depth depth)
    {
        var list = GetSpawnTable(habitat, depth);
        if (list == null || list.Count == 0)
        {
            Debug.LogError($"FishManager.GetFishByHabitatDepth: empty table for {habitat}/{depth}");
            return new FishBase();
        }
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    // Prefer FishSpawner; fallback to legacy prefab list
    private GameObject InstantiateFishById(int fishId, Vector3 pos)
    {
        if (fishSpawner != null)
            return fishSpawner.SpawnById(fishId, pos);

        if (fishPrefabList != null && fishId > 0 && fishId <= fishPrefabList.Count)
        {
            var prefab = fishPrefabList[fishId - 1];
            if (prefab != null)
                return Instantiate(prefab, pos, Quaternion.identity);
        }

        Debug.LogWarning($"FishManager: prefab not found for id {fishId}, using error prefab.");
        return Instantiate(errorFishPrefab, pos, Quaternion.identity);
    }

    private List<FishBase> GetCollectionForCurrentHabitat()
    {
        switch (Player.Instance._GetCurrentHabitat)
        {
            case Habitat.Normal:
                return habitatNormalFishBaseList;
            case Habitat.Volcanic:
                return habitatVolcanicFishBaseList;
            case Habitat.Arctic:
                return habitatArcticFishBaseList;
            case Habitat.Tropical:
                return habitatTropicalFishBaseList;
            default:
                return habitatNormalFishBaseList;
        }
    }

    private Rarity DetermineRarityFromRoll(int roll)
    {
        if (roll >= (100 - Common.GameplayValues.LEGENDARY_FISH_SPAWN_PERCENT))
            return Rarity.Legendary;
        if (roll >= (100 - Common.GameplayValues.EPIC_FISH_SPAWN_PERCENT))
            return Rarity.Epic;
        if (roll >= (100 - Common.GameplayValues.RARE_FISH_SPAWN_PERCENT))
            return Rarity.Rare;
        if (roll >= (100 - Common.GameplayValues.UNCOMMON_FISH_SPAWN_PERCENT))
            return Rarity.Uncommon;
        return Rarity.Common;
    }

    private bool IsAllowedBySize(FishType type)
    {
        return (type != FishType.Small || allowSmall) &&
               (type != FishType.Medium || allowMedium) &&
               (type != FishType.Large || allowLarge) &&
               (type != FishType.Leviathan || allowLeviathan);
    }

    // Legacy helper retained for compatibility
    [Obsolete("Use SpawnById or FishSpawner.SpawnById instead.")]
    private GameObject GetFishPrefabByID(int id)
    {
        if (fishPrefabList != null && id > 0 && id <= fishPrefabList.Count)
            return fishPrefabList[id - 1];
        return errorFishPrefab;
    }

    // Internal helper to return the legacy habitat list for DB fallback use
    private List<FishBase> GetCollectionForCurrentHabitatInternal(Habitat habitat)
    {
        switch (habitat)
        {
            case Habitat.Normal:
                return habitatNormalFishBaseList;
            case Habitat.Volcanic:
                return habitatVolcanicFishBaseList;
            case Habitat.Arctic:
                return habitatArcticFishBaseList;
            case Habitat.Tropical:
                return habitatTropicalFishBaseList;
            default:
                return habitatNormalFishBaseList;
        }
    }
}
