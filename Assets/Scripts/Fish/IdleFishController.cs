using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleFishController : MonoBehaviour
{
    [Header("Visuals & Materials")]
    
    public SkinnedMeshRenderer fishMeshRenderer;// Drag the MeshRenderer (usually a child object of the prefab) into this slot

    // An array to hold the materials for this specific species.
    // Order them in the Inspector to match your Rarity Enum (0=Common, 1=Uncommon, etc.)
    public Material[] rarityMaterials;

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

    //If we add the Leviathan class fish that should just wander alone, we can drop its cohesionWeight and alignmentWeight to 0 in its prefab.
    [Header("Boid Settings")]
    public float neighborRadius = 3.0f;    // How far it can see flockmates
    public float avoidanceRadius = 1.0f;   // How close is "too close"
    public float maxSteerForce = 2.0f;     // How fast it can turn

    [Header("Boid Weights")]
    public float separationWeight = 1.5f;  // Keep away from everyone
    public float alignmentWeight = 1.0f;   // Face the same way as my species
    public float cohesionWeight = 1.0f;    // Stay near my species
    public float boundsWeight = 5.0f;      // Don't hit the glass!

    private Vector3 _velocity;

    /// <summary>
    /// Called by IdleTankManager immediately after instantiating the prefab.
    /// </summary>
    public void Initialize(FishSaveRecord saveRecord, FishData templateData)
    {
        _baseStats = templateData;
        _instanceStats = saveRecord;

        FishName = _baseStats.Name;
        FishRarity = _baseStats.Rarity;
        //Assign Instance Data (From the Save File)
        CurrentWeight = _instanceStats.LastWeight;

        // Grab the base speed from their Kaiju Fishing catch
        float baseSpeed = _instanceStats.LastSpeed;
        // Apply a random variance between 80% and 120% of their base speed
        float randomVariance = Random.Range(0.8f, 1.2f);
        SwimmingSpeed = baseSpeed * randomVariance;

        float weightMultiplier = Mathf.Max(1f, CurrentWeight * _baseStats.WeightClassMultiplier);
        BoonsPerSecond = (_baseStats.BoonBonus * weightMultiplier) / 60f;

        if (fishMeshRenderer != null && rarityMaterials.Length > 0)
        {
            // Cast the enum to an integer to find the right material in the array
            int rarityIndex = (int)FishRarity;

            // Safety check in case the array doesn't have enough materials assigned
            if (rarityIndex < rarityMaterials.Length)
            {
                fishMeshRenderer.material = rarityMaterials[rarityIndex];
            }
            else
            {
                Debug.LogWarning($"{FishName} is missing a material for rarity {FishRarity}!");
            }
        }

        ApplyVisualScale();

        // Give the fish an initial push in a random direction
        _velocity = Random.onUnitSphere * (SwimmingSpeed * 0.1f);
    }

    private void ApplyVisualScale()
    {
        // Simple scaling math so heavier fish are physically larger in the tank
        float scaleFactor = 1f + (CurrentWeight * 0.05f);
        transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

        //Make the fish's personal bubble grow with its physical size!
        //(Assuming a base avoidance radius of 1.5f) - This may need tweaking to feel right, especially for very large fish, but it's a good starting point.
        avoidanceRadius = 1.5f * scaleFactor;
    }

    private void Update()
    {
        if (_baseStats == null || IdleTankManager.Instance == null) return;

        ApplyBoidRules();
        MoveFish();
    }

    // This allows a global GameManager to harvest the generated Boons
    public float CollectGeneratedBoons(float timeElapsedSeconds)
    {
        return BoonsPerSecond * timeElapsedSeconds;
    }

    private void ApplyBoidRules()
    {
        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;

        int flockCount = 0;
        int avoidCount = 0;

        // Loop through all active fish in the tank
        foreach (IdleFishController otherFish in IdleTankManager.Instance.ActiveFish)
        {
            if (otherFish == this || otherFish == null) continue;

            float dist = Vector3.Distance(transform.position, otherFish.transform.position);

            // Rule 1: Separation (Avoid ALL fish, regardless of species, so they don't clip)
            if (dist < avoidanceRadius)
            {
                Vector3 diff = transform.position - otherFish.transform.position;
                separation += diff.normalized / dist; // Weight heavily by how close they are
                avoidCount++;
            }

            // Rules 2 & 3: Alignment & Cohesion (Only school with the exact same FishName)
            if (dist < neighborRadius && otherFish.FishName == this.FishName)
            {
                alignment += otherFish._velocity;
                cohesion += otherFish.transform.position;
                flockCount++;
            }
        }

        // Calculate averages and steering targets
        if (flockCount > 0)
        {
            // Alignment: Steer towards average velocity of flock
            alignment /= flockCount;
            alignment = alignment.normalized * (SwimmingSpeed * 0.1f);
            alignment = Vector3.ClampMagnitude(alignment - _velocity, maxSteerForce);

            // Cohesion: Steer towards center of mass of flock
            cohesion /= flockCount;
            cohesion = (cohesion - transform.position).normalized * (SwimmingSpeed * 0.1f);
            cohesion = Vector3.ClampMagnitude(cohesion - _velocity, maxSteerForce);
        }

        if (avoidCount > 0)
        {
            // Separation: Steer away from crowded areas
            separation /= avoidCount;
            separation = separation.normalized * (SwimmingSpeed * 0.1f);
            separation = Vector3.ClampMagnitude(separation - _velocity, maxSteerForce);
        }

        // Rule 4: Containment (Don't hit the glass)
        Vector3 boundsForce = AvoidBounds();

        // Combine all forces
        Vector3 acceleration = (separation * separationWeight) +
                               (alignment * alignmentWeight) +
                               (cohesion * cohesionWeight) +
                               (boundsForce * boundsWeight);

        // Apply acceleration to velocity, but clamp it so they don't break the sound barrier
        _velocity += acceleration * Time.deltaTime;
        _velocity = Vector3.ClampMagnitude(_velocity, SwimmingSpeed * 0.1f);
    }

    private Vector3 AvoidBounds()
    {
        Bounds b = IdleTankManager.Instance.TankBounds;
        Vector3 pos = transform.position;
        Vector3 force = Vector3.zero;

        // How close to the glass before they panic and turn around
        float edgePadding = 1.5f;

        if (pos.x < b.min.x + edgePadding) force.x = 1;
        else if (pos.x > b.max.x - edgePadding) force.x = -1;

        if (pos.y < b.min.y + edgePadding) force.y = 1;
        else if (pos.y > b.max.y - edgePadding) force.y = -1;

        if (pos.z < b.min.z + edgePadding) force.z = 1;
        else if (pos.z > b.max.z - edgePadding) force.z = -1;

        // If they are hitting a wall, steer away hard!
        return force.normalized * maxSteerForce;
    }

    private void MoveFish()
    {
        // Physically move the fish
        transform.position += _velocity * Time.deltaTime;

        // Smoothly rotate to face the direction they are swimming
        if (_velocity != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_velocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 3f);
        }
    }
}
