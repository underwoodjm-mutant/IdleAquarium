using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralized save service with legacy migration template.
/// - Keeps same ES3 save file name.
/// - Migrates legacy records into compact FishSaveRecord list on first load.
/// Adjust legacy DTO fields to match what your current ES3 file contains.
/// </summary>
public static class SaveService
{
    private const string SaveFile = "KFSaveData.es3";
    private const string FishCollectionKey = "Player.FishCollection";
    private const string SaveVersionKey = "Save.Version";
    private const int CurrentSaveVersion = 2;

    public static void LoadAll()
    {
        try
        {
            int version = 0;
            if (ES3.KeyExists(SaveVersionKey, SaveFile))
                version = ES3.Load<int>(SaveVersionKey, SaveFile);

            if (version < CurrentSaveVersion)
            {
                MigrateFromLegacy(version);
                ES3.Save(SaveVersionKey, CurrentSaveVersion, SaveFile);
            }

            List<FishSaveRecord> fish = ES3.KeyExists(FishCollectionKey, SaveFile)
                ? ES3.Load<List<FishSaveRecord>>(FishCollectionKey, SaveFile)
                : new List<FishSaveRecord>();

            PlayerDataManager.Instance.ApplyFishSaves(fish);
        }
        catch (Exception ex)
        {
            Debug.LogError($"SaveService.LoadAll failed: {ex}");
        }
    }

    private static void MigrateFromLegacy(int version)
    {
        // Example migration: legacy saved a list of LegacyFishRecord under the same key.
        // Adjust `LegacyFishRecord` fields to match what ES3 actually stored previously.
        if (!ES3.KeyExists(FishCollectionKey, SaveFile)) return;

        try
        {
            var legacy = ES3.Load<List<LegacyFishRecord>>(FishCollectionKey, SaveFile);
            var migrated = new List<FishSaveRecord>(legacy.Count);
            foreach (var l in legacy)
            {
                migrated.Add(new FishSaveRecord
                {
                    Id = l.FishID,
                    Discovered = l.Discovered,
                    NumberCaught = l.NumberCaught,
                    LastWeight = l.FishWeight,
                    LastSpeed = l.FishSpeed
                });
            }

            ES3.Save(FishCollectionKey, migrated, SaveFile);
            Debug.Log($"SaveService: migrated {migrated.Count} fish records from legacy save.");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"SaveService.MigrateFromLegacy failed: {ex}");
            // If migration failed, do not destroy legacy data; allow debugging.
        }
    }

    // Legacy DTO - must reflect your previously serialized structure when using ES3.
    [Serializable]
    private class LegacyFishRecord
    {
        public int FishID;
        public bool Discovered;
        public int NumberCaught;
        public float FishWeight;
        public float FishSpeed;
        // If other fields were serialized, add them here exactly as named.
    }

    public static void SaveFishCollection(List<FishSaveRecord> fish)
    {
        try
        {
            ES3.Save(FishCollectionKey, fish, SaveFile);
        }
        catch (Exception ex)
        {
            Debug.LogError($"SaveService.SaveFishCollection failed: {ex}");
        }
    }
}
