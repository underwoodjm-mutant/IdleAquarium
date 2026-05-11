using System;

[Serializable]
public class ActiveBountyRecord
{
    public string InstanceId; // A unique ID in case they run multiple bounties later
    public BountyTier Tier;
    public Habitat TargetHabitat;
    public DateTime CompletionTime;
    public float Cost;
}
