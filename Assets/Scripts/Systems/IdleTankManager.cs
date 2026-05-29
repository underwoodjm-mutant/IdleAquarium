using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class IdleTankManager : Singleton<IdleTankManager>
{
    [Header("Tank Limits")]
    public int maxCapacity = 5; // Upgradable later!
    private List<GameObject> _activeFishInTank = new List<GameObject>();

    [Header("Spawning References")]
    public BoxCollider tankSpawnVolume;
    [SerializeField] private Vector3 _spawnAreaSize = new Vector3(10, 5, 10);

    [Header("Data References")]
    [SerializeField] private FishDatabase _fishDatabase;
    [SerializeField] private GameObject _defaultFishPrefab; // The visual fish for the idle game
    

    // Exact constants from the Kaiju Fishing SaveService.cs
    private const string SaveFile = "KFSaveData.es3";
    private const string FishCollectionKey = "Player.FishCollection";

    // Tracks all spawned fish so the EconomyManager can read their stats
    public List<IdleFishController> ActiveFish { get; private set; } = new List<IdleFishController>();

    public Bounds TankBounds => tankSpawnVolume.bounds;

    public override void Awake()
    {
        // Crucial: Call base to enforce the Singleton and DontDestroyOnLoad logic
        base.Awake();
    }

    private void Start()
    {
        LoadAndSpawnTank();
    }

    public bool HasSpaceInTank()
    {
        return _activeFishInTank.Count < maxCapacity;
    }

    public void LoadAndSpawnTank()
    {
        // 1. Check if the Kaiju Fishing save file exists
        if (!ES3.KeyExists(FishCollectionKey, SaveFile))
        {
            Debug.LogWarning("No cross-game save file found. Start Kaiju Fishing first!");
            return;
        }

        // 2. Load the lightweight saved records
        List<FishSaveRecord> savedFishList = ES3.Load<List<FishSaveRecord>>(FishCollectionKey, SaveFile);
        Debug.Log($"Loaded {savedFishList.Count} fish from Kaiju Fishing save.");

        // 3. Iterate and build the complete fish
        foreach (FishSaveRecord record in savedFishList)
        {
            // We only want to spawn fish the player has actually caught/discovered
            if (!record.Discovered || record.NumberCaught <= 0) continue;

            // Look up the static stats from the CSV-backed database
            FishData templateData = _fishDatabase.GetById(record.Id);

            if (templateData == null)
            {
                Debug.LogError($"Fish ID {record.Id} found in save, but missing from CSV Database!");
                continue;
            }

            // Spawn the fish using BOTH sets of data
            SpawnIdleFish(record, templateData);
        }
    }

    private void SpawnIdleFish(FishSaveRecord saveRecord, FishData templateData)
    {
        // Pick a random spot in the tank
        Vector3 randomPos = transform.position + new Vector3(
            Random.Range(-_spawnAreaSize.x / 2, _spawnAreaSize.x / 2),
            Random.Range(-_spawnAreaSize.y / 2, _spawnAreaSize.y / 2),
            Random.Range(-_spawnAreaSize.z / 2, _spawnAreaSize.z / 2)
        );

        // Instantiate the visual fish
        GameObject fishObj = Instantiate(_defaultFishPrefab, randomPos, Quaternion.identity, transform);

        // Initialize the fish with both the Save Data and the CSV Data
        IdleFishController controller = fishObj.GetComponent<IdleFishController>();
        if (controller != null)
        {
            controller.Initialize(saveRecord, templateData);
            ActiveFish.Add(controller); // Track it for the EconomyManager
        }

        fishObj.name = $"IdleFish_{templateData.Name}_{templateData.Rarity}";
        Debug.Log($"Spawned {templateData.Name}! It weighs {saveRecord.LastWeight} and generates {(controller != null ? controller.BoonsPerSecond : 0):F2} Boons/sec.");
    }

    // Helper method used by EconomyManager to calculate idle gains
    public float GetTotalBoonsPerSecond()
    {
        float total = 0f;
        foreach (var fish in ActiveFish)
        {
            total += fish.BoonsPerSecond;
        }
        return total;
    }


    public void SyncIdleUnlockToMainGame(int newFishId, float weight, float speed)
    {
        string sharedSave = "KFSaveData.es3";
        string key = "Player.FishCollection";

        // Always do a fresh read right before writing to ensure we don't 
        // overwrite anything the player just did in the main game!
        List<FishSaveRecord> currentCollection = ES3.Load(key, sharedSave, new List<FishSaveRecord>());

        //Find the fish or create a new record
        FishSaveRecord existingRecord = currentCollection.Find(f => f.Id == newFishId);

        if (existingRecord != null)
        {
            // They already have it, just increment the idle counter
            existingRecord.NumberIncubated++;

            // Optional: Only update stats if the idle-generated one is better
            if (weight > existingRecord.LastWeight) existingRecord.LastWeight = weight;
            if (speed > existingRecord.LastSpeed) existingRecord.LastSpeed = speed;
        }
        else
        {
            // Brand new unlock via the idle game!
            currentCollection.Add(new FishSaveRecord
            {
                Id = newFishId,
                Discovered = true,
                NumberCaught = 0, // CRITICAL: Stays 0 so bosses don't unlock
                NumberIncubated = 1,
                LastWeight = weight,
                LastSpeed = speed
            });
        }

        SpawnFishVisually(newFishId, weight, speed);

        //Write back to the shared save
        ES3.Save(key, currentCollection, sharedSave);
        Debug.Log($"Successfully synced Idle Unlock (ID: {newFishId}) to Kaiju Fishing save!");
    }

    private void SpawnFishVisually(int fishId, float weightMod, float speed)
    {
        if (tankSpawnVolume == null)
        {
            Debug.LogError("Tank Manager is missing prefabs or spawn volume!");
            return;
        }

        //Fetch the Template Data
        FishData templateData = FishDatabase.Instance.GetById(fishId);

        if (templateData == null) return;
        if (templateData.IdlePrefab == null)
        {
            Debug.LogError($"FishData for {templateData.Name} is missing its IdlePrefab!");
            return;
        }

        //Find a random coordinate inside the BoxCollider
        Vector3 randomPos = GetRandomPositionInWater();

        //Spawn the prefab
        GameObject newFishObj = Instantiate(templateData.IdlePrefab, randomPos, Quaternion.identity, transform);

        //Apply the modified weight size
        // If the fish is 20% heavier, make it 20% larger in 3D space!
        newFishObj.transform.localScale = Vector3.one * weightMod;

        
        if (templateData == null) return;

        //Construct a mock SaveRecord for the instance data
        FishSaveRecord newRecord = new FishSaveRecord
        {
            LastWeight = templateData.Weight * weightMod,
            LastSpeed = speed
        };

        IdleFishController idleController = newFishObj.GetComponent<IdleFishController>();
        if (idleController != null)
        {
            // This sets up the BoonsPerSecond and scales the 3D model!
            idleController.Initialize(newRecord, templateData);
            ActiveFish.Add(idleController); // Track it for the EconomyManager
        }
        else
        {
            Debug.LogError("The spawned prefab is missing the IdleFishController component!");
        }

        //Attach data to the FishBase script if it has one
        FishBase fb = newFishObj.GetComponent<FishBase>();
        if (fb != null)
        {
            fb.FishID = fishId;
            fb.MyState = FishState.Swim; // Set them to immediately start idle swimming
        }

        //Track it for capacity limits
        _activeFishInTank.Add(newFishObj);
    }

    private Vector3 GetRandomPositionInWater()
    {
        Bounds bounds = tankSpawnVolume.bounds;
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, _spawnAreaSize);
    }
}
