using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "FishDatabase", menuName = "Game/Data/FishDatabase")]
public class FishDatabase : ScriptableObject
{
    public List<FishData> fishes = new List<FishData>();

    // --- THE PSEUDO-SINGLETON ---
    private static FishDatabase _instance;

    public static FishDatabase Instance
    {
        get
        {
            // If we haven't found it yet this session, go look for it
            if (_instance == null)
            {
                // This searches all "Resources" folders for an asset named "FishDatabase"
                _instance = Resources.Load<FishDatabase>("FishDatabase");

                if (_instance == null)
                {
                    Debug.LogError("FishDatabase not found! Make sure the asset is named 'FishDatabase' and is inside a 'Resources' folder.");
                }
            }
            return _instance;
        }
    }

    public FishData GetById(int id)
    {
        return fishes.Find(f => f != null && f.Id == id);
    }

    public List<FishData> GetByRarity(Rarity r)
    {
        return fishes.FindAll(f => f != null && f.Rarity == r);
    }
    public List<FishData> GetByHabitat(Habitat targetHabitat)
    {
        return fishes.Where(f => f.Habitat == targetHabitat && f.Type != FishType.Leviathan).ToList();
    }
}
