using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BountyBoardManager : Singleton<BountyBoardManager>
{
    [Header("Bounty Costs (In Lumins)")]
    public float OpenContractCost = 1000f;
    public float VeteranContractCost = 15000f;
    public float EliteWarrantCost = 100000f;

    [Header("Bounty Wait Times (In Seconds)")]
    public float OpenContractTime = 5f;
    public float VeteranContractTime = 15f;
    public float EliteWarrantTime = 30f;

    // The currently running bounty (For now, just one at a time)
    private ActiveBountyRecord _currentBounty;
    private const string SAVE_KEY = "Idle_ActiveBounty";

    // --- EVENTS ---
    // Fires immediately when money is spent. Passes the wait time to the UI.
    public event Action<ActiveBountyRecord> OnBountyStarted;
    // Fires when the delay finishes and the fish is generated.
    //public event Action<int, RarityLevel> OnBountyFulfilled;
    public event Action OnBountyReadyToReveal;
    public event Action<bool> OnRewardProcessed; // True if kept, False if released

    // --- WEIGHTED DROP TABLES ---
    private readonly int[] _openContractWeights = { 80, 15, 4, 1, 0 };
    private readonly int[] _veteranContractWeights = { 40, 30, 20, 9, 1 };
    private readonly int[] _eliteWarrantWeights = { 0, 20, 40, 30, 10 };

    private PendingReward _pendingReward;

    // --- DEV HACK: TIME TRAVEL ---
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void OnGUI()
    {
        // A tiny on-screen button to instantly finish the timer while testing
        if (_currentBounty != null && GUI.Button(new Rect(10, 10, 150, 50), "HACK: Finish Bounty"))
        {
            _currentBounty.CompletionTime = DateTime.UtcNow;
        }
    }
#endif

    private void Start()
    {
        LoadBountyState();
    }

    private void Update()
    {
        // If we have an active bounty, check the clock every frame
        if (_currentBounty != null)
        {
            if (DateTime.UtcNow >= _currentBounty.CompletionTime)
            {
                //ExecuteBountyFulfillment(_currentBounty);
                // The Angler is back! Roll the dice, but DON'T save it yet.
                PreparePendingReward(_currentBounty);
            }
        }
    }

    private void PreparePendingReward(ActiveBountyRecord bounty)
    {
        // 1. Roll for Rarity & ID
        RarityLevel rolledRarity = RollForRarity(bounty.Tier);
        int rolledFishId = GetRandomFishIdByHabitat(bounty.TargetHabitat);
        float randomWeightMod = Random.Range(0.8f, 1.2f);

        // 2. Store it in Purgatory
        _pendingReward = new PendingReward
        {
            FishId = rolledFishId,
            Rarity = rolledRarity,
            WeightModifier = randomWeightMod,
            CostPaid = bounty.Cost
        };

        // 3. Clear the active bounty state so the timer stops
        _currentBounty = null;
        ES3.DeleteKey(SAVE_KEY);

        // 4. Tell the UI to show the Mystery Box!
        Debug.Log("Angler has returned! Awaiting player reveal.");
        OnBountyReadyToReveal?.Invoke();
    }
    /// <summary>
    /// Called by the UI when a player purchases a bounty.
    /// </summary>
    public bool PostBounty(BountyTier tier, Habitat targetHabitat)
    {
        if (_currentBounty != null)
        {
            Debug.LogWarning("A bounty is already in progress!");
            return false;
        }

        float cost = GetCostForTier(tier);

        // 1. Verify Funds
        if (EconomyManager.Instance.BankedBoons < cost)
        {
            Debug.LogWarning("Not enough Lumins to post this bounty!");
            return false;
        }

        // 2. Deduct Funds Immediately
        EconomyManager.Instance.BankedBoons -= cost;

        // 3. Get the Delay Time
        float waitTime = GetWaitTimeForTier(tier);
        _currentBounty = new ActiveBountyRecord
        {
            InstanceId = System.Guid.NewGuid().ToString(),
            Tier = tier,
            TargetHabitat = targetHabitat,
            CompletionTime = DateTime.UtcNow.AddSeconds(waitTime),
            Cost = cost
        };

        // 4. Save it immediately so they don't lose it if the game crashes!
        SaveBountyState();

        // 4. Tell the UI the hunt has begun
        OnBountyStarted?.Invoke(_currentBounty);

        return true;
    }

    

    // --- SAVE / LOAD LOGIC ---
    private void SaveBountyState()
    {
        // Saving to the dedicated Idle Save file so we don't touch KaijuFishing's file
        ES3.Save(SAVE_KEY, _currentBounty, "IdleSaveData.es3");
    }

    private void LoadBountyState()
    {
        if (ES3.KeyExists(SAVE_KEY, "IdleSaveData.es3"))
        {
            _currentBounty = ES3.Load<ActiveBountyRecord>(SAVE_KEY, "IdleSaveData.es3");

            // If we loaded a bounty, tell the UI to show the "In Progress" screen
            // Even if the time has passed, the Update loop will catch it on the next frame!
            if (_currentBounty != null)
            {
                OnBountyStarted?.Invoke(_currentBounty);
            }
        }
    }
    /*
     * Reworked into a lootbox reward system, so we don't need this anymore. Keeping it here for reference in case we want to add timed bounties later.
     * 
    private IEnumerator HuntRoutine(BountyTier tier, Habitat targetHabitat, float waitTime, float cost)
    {
        // --- THE DELAY ---
        // Unity will pause this specific method right here for X seconds, 
        // while the rest of the game keeps running!
        yield return new WaitForSeconds(waitTime);

        // --- THE RESULT (Runs after the wait) ---

        // Roll for Rarity
        RarityLevel rolledRarity = RollForRarity(tier);

        // Roll for a valid Fish ID
        int rolledFishId = GetRandomFishIdByHabitat(targetHabitat);
        if (rolledFishId == -1)
        {
            Debug.LogError($"No valid fish found for habitat: {targetHabitat}. Refund required.");
            EconomyManager.Instance.BankedBoons += cost; // Refund on error
            yield break; // Exit the coroutine
        }

        // Generate a random Weight Modifier (e.g., +/- 20% of base size)
        float randomWeightMod = Random.Range(0.8f, 1.2f);

        // Execute the Sync
        IdleTankManager.Instance.SyncIdleUnlockToMainGame(rolledFishId, randomWeightMod, (int)rolledRarity);

        Debug.Log($"Scavenger returned! Acquired Fish ID {rolledFishId} at {rolledRarity} rarity.");

        // Tell the UI to show the result pop-up!
        OnBountyFulfilled?.Invoke(rolledFishId, rolledRarity);
    }
    private void ExecuteBountyFulfillment(ActiveBountyRecord bounty)
    {
        // 1. Clear the active bounty state FIRST to prevent double-firing in Update
        _currentBounty = null;
        ES3.DeleteKey(SAVE_KEY); // Remove the save file key

        // 2. Roll for Rarity & ID
        RarityLevel rolledRarity = RollForRarity(bounty.Tier);
        int rolledFishId = GetRandomFishIdByHabitat(bounty.TargetHabitat);

        if (rolledFishId == -1)
        {
            Debug.LogError("No valid fish found. Refunding.");
            EconomyManager.Instance.BankedBoons += bounty.Cost;
            return;
        }

        // 3. Execute the Sync
        float randomWeightMod = Random.Range(0.8f, 1.2f);
        IdleTankManager.Instance.SyncIdleUnlockToMainGame(rolledFishId, randomWeightMod, (int)rolledRarity);

        Debug.Log($"Scavenger returned! Fish ID {rolledFishId} at {rolledRarity} rarity.");
        OnBountyFulfilled?.Invoke(rolledFishId, rolledRarity);
    }
    */
    public PendingReward GetPendingReward()
    {
        return _pendingReward;
    }

    public void ProcessPlayerChoice(bool keepFish)
    {
        if (_pendingReward == null) return;

        if (keepFish)
        {
            // Player kept it. NOW we sync to the main game save file.
            IdleTankManager.Instance.SyncIdleUnlockToMainGame(
                _pendingReward.FishId,
                _pendingReward.WeightModifier,
                (int)_pendingReward.Rarity);

            Debug.Log("Player kept the fish. Synced to KFSaveData.");
        }
        else
        {
            // Player released it. Calculate a moderate refund (e.g., 40% of the cost).
            float refundAmount = _pendingReward.CostPaid * 0.4f;
            EconomyManager.Instance.BankedBoons += refundAmount;

            Debug.Log($"Player released the fish. Refunded {refundAmount} Lumins.");
        }

        // Clear the purgatory slot so they can send another Angler
        _pendingReward = null;
        OnRewardProcessed?.Invoke(keepFish);
    }
    // --- HELPER METHODS ---

    public ActiveBountyRecord GetActiveBounty()
    {
        return _currentBounty;
    }

    private float GetCostForTier(BountyTier tier)
    {
        switch (tier)
        {
            case BountyTier.OpenContract: return OpenContractCost;
            case BountyTier.VeteranContract: return VeteranContractCost;
            case BountyTier.EliteWarrant: return EliteWarrantCost;
            default: return 0f;
        }
    }

    private float GetWaitTimeForTier(BountyTier tier)
    {
        switch (tier)
        {
            case BountyTier.OpenContract: return OpenContractTime;
            case BountyTier.VeteranContract: return VeteranContractTime;
            case BountyTier.EliteWarrant: return EliteWarrantTime;
            default: return 5f;
        }
    }

    private RarityLevel RollForRarity(BountyTier tier)
    {
        int[] activeWeights;
        switch (tier)
        {
            case BountyTier.OpenContract: activeWeights = _openContractWeights; break;
            case BountyTier.VeteranContract: activeWeights = _veteranContractWeights; break;
            case BountyTier.EliteWarrant: activeWeights = _eliteWarrantWeights; break;
            default: activeWeights = _openContractWeights; break;
        }

        int roll = Random.Range(0, 100);
        int cumulativeWeight = 0;

        for (int i = 0; i < activeWeights.Length; i++)
        {
            cumulativeWeight += activeWeights[i];
            if (roll < cumulativeWeight)
            {
                switch (i)
                {
                    case 0: return RarityLevel.Common;
                    case 1: return RarityLevel.Uncommon;
                    case 2: return RarityLevel.Rare;
                    case 3: return RarityLevel.Epic;
                    case 4: return RarityLevel.Legendary;
                }
            }
        }
        return RarityLevel.Common;
    }

    private int GetRandomFishIdByHabitat(Habitat habitat)
    {
        List<FishData> validFish = FishDatabase.Instance.GetByHabitat(habitat);
        if (validFish == null || validFish.Count == 0) return -1;
        int randomIndex = Random.Range(0, validFish.Count);
        return validFish[randomIndex].Id;
    }
}