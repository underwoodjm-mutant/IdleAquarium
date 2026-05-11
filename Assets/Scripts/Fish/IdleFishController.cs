using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleFishController : MonoBehaviour
{
    [Header("Merged Data")]
    public string FishName;
    public Rarity FishRarity;
    public float CurrentWeight;
    public float SwimmingSpeed;

    [Header("Idle Economy")]
    public float BoonsPerSecond;

    // Optional: Store the raw data if you need to reference it later for UI
    private FishData _baseStats;
    private FishSaveRecord _instanceStats;

    /// <summary>
    /// Called by IdleTankManager immediately after instantiating the prefab.
    /// </summary>
    public void Initialize(FishSaveRecord saveRecord, FishData templateData)
    {
        _baseStats = templateData;
        _instanceStats = saveRecord;

        //Assign Visual/Identity Data (From the CSV Template)
        FishName = _baseStats.Name;
        FishRarity = _baseStats.Rarity;

        //Assign Instance Data (From the Save File)
        CurrentWeight = _instanceStats.LastWeight;
        SwimmingSpeed = _instanceStats.LastSpeed;

        //Calculate Idle Game Economy
        // Example: Base BoonBonus multiplied by the specific fish's weight!
        // A heavier Flamander generates more Boons than a lighter one.
        float weightMultiplier = Mathf.Max(1f, CurrentWeight * _baseStats.WeightClassMultiplier);
        BoonsPerSecond = (_baseStats.BoonBonus * weightMultiplier) / 60f;

        // 4. Apply scale based on weight (Optional but fun for Kaiju scale)
        ApplyVisualScale();
    }

    private void ApplyVisualScale()
    {
        // Simple scaling math so heavier fish are physically larger in the tank
        float scaleFactor = 1f + (CurrentWeight * 0.05f);
        transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
    }

    private void Update()
    {
        if (_baseStats == null) return;

        // Very basic forward movement using the Speed saved from Kaiju Fishing
        // In a full implementation, you would add rotation/wandering logic here.
        transform.Translate(Vector3.forward * (SwimmingSpeed * 0.1f) * Time.deltaTime);
    }

    // This allows a global GameManager to harvest the generated Boons
    public float CollectGeneratedBoons(float timeElapsedSeconds)
    {
        return BoonsPerSecond * timeElapsedSeconds;
    }
}
