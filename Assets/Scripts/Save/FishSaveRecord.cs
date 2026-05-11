using System;

[Serializable]
public class FishSaveRecord
{
    public int Id;
    public bool Discovered;
    public int NumberCaught;
    public float LastWeight;
    public float LastSpeed;
    public int NumberIncubated;
    public int RarityLevel;
    public bool IsInCollection => NumberCaught > 0 || NumberIncubated > 0;
}