using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime lightweight player data manager. Holds compact fish save records and exposes simple lookup.
/// This keeps saved state decoupled from scene MonoBehaviours.
/// </summary>
public class PlayerDataManager : Singleton<PlayerDataManager>
{
    public List<FishSaveRecord> OwnedFish = new List<FishSaveRecord>();

    public override void Awake()
    {
        base.Awake();
        // Load saved data when the manager initializes.
        SaveService.LoadAll();
    }

    public void ApplyFishSaves(List<FishSaveRecord> records)
    {
        OwnedFish = records ?? new List<FishSaveRecord>();
    }

    public FishSaveRecord GetFishRecord(int id)
    {
        return OwnedFish.Find(f => f.Id == id);
    }

    public void Save()
    {
        SaveService.SaveFishCollection(OwnedFish);
    }
}