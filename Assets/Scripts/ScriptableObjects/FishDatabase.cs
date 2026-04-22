using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FishDatabase", menuName = "Game/Data/FishDatabase")]
public class FishDatabase : ScriptableObject
{
    public List<FishData> fishes = new List<FishData>();

    public FishData GetById(int id)
    {
        return fishes.Find(f => f != null && f.Id == id);
    }

    public List<FishData> GetByRarity(Rarity r)
    {
        return fishes.FindAll(f => f != null && f.Rarity == r);
    }
}
