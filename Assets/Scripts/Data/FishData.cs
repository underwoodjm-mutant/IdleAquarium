using System;
using UnityEngine;

[Serializable]
public class FishData
{
    public int Id;
    public string Name;
    public string Description;
    public FishType Type;
    public Habitat Habitat;
    public int CatchTier;
    public Attractor Attractor;
    public Rarity Rarity;
    public int SpecialLootId;
    public Difficulty Difficulty;
    public float WeightClassMultiplier;
    public int Experience;
    public string MovementDescription;
    public int CatchMissionCount1;
    public int CatchMissionCount2;
    public int CatchMissionCount3;
    public int BoonBonus;

    // runtime-only values
    public float Weight;
    public float Speed;
    public int NumberCaught;
    public bool Discovered;

    [Header("Visuals")]
    public Sprite Icon;
    public Sprite BG;
    public GameObject IdlePrefab;//The specific 3D model prefab for this species
}
