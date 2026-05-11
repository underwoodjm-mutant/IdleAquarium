using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : Singleton<EconomyManager>
{

    [Header("Economy Data")]
    public float BankedBoons;           // Money the player has collected and can spend
    public float UncollectedBoons;      // Money floating in the tank waiting to be clicked
    public float MaxTankCapacity = 500f;// Base capacity (Can be upgraded later!)

    // Actions are like broadcast channels. Other scripts can "tune in" to listen.
    public event Action<float> OnBankChanged;
    public event Action<float, float> OnTankFilled;

    // Timer to throttle continuous UI updates (so we don't update 144 times a second)
    private float _uiUpdateTimer = 0f;
    private const float UI_UPDATE_INTERVAL = 0.1f; // Update UI 10 times a second

    // Dedicated save file for the Idle Game
    private const string IdleSaveFile = "IdleSaveData.es3";
    private const string LastSaveTimeKey = "Idle.LastSaveTime";

    // ==========================================
    // --- DEV HACKS (Editor & Dev Builds Only) ---
    // ==========================================
#if UNITY_EDITOR || DEVELOPMENT_BUILD

    [Header("Dev Hacks")]
    public float devHackAmount = 50000f;

    // The ContextMenu attribute allows you to right-click the EconomyManager 
    // script in the Unity Inspector and trigger this method manually!
    [ContextMenu("HACK: Add Dev Lumins")]
    private void InjectDevBoons()
    {
        BankedBoons += devHackAmount;
        Debug.LogWarning($"[DEV HACK] Added {devHackAmount} Lumins! Total is now {BankedBoons}.");

        // Tell the UI to update immediately
        OnBankChanged?.Invoke(BankedBoons);
    }

#endif
    // ==========================================
    public override void Awake()
    {
        // Crucial: Call the base Awake so the Singleton handles instantiation 
        // and DontDestroyOnLoad logic properly.
        base.Awake();
    }

    private void Start()
    {
        // 1. Load saved economy data
        LoadEconomy();

        // 2. Calculate what happened while the game was closed
        CalculateOfflineProgress();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            InjectDevBoons();
        }

        if (UncollectedBoons < MaxTankCapacity && IdleTankManager.Instance != null)
        {
            float productionThisFrame = IdleTankManager.Instance.GetTotalBoonsPerSecond() * Time.deltaTime;
            UncollectedBoons += productionThisFrame;

            if (UncollectedBoons > MaxTankCapacity)
            {
                UncollectedBoons = MaxTankCapacity;
            }

            //THROTTLED EVENT BROADCAST
            // Instead of updating UI every frame, broadcast the new tank values 10x a second.
            _uiUpdateTimer += Time.deltaTime;
            if (_uiUpdateTimer >= UI_UPDATE_INTERVAL)
            {
                OnTankFilled?.Invoke(UncollectedBoons, MaxTankCapacity);
                _uiUpdateTimer = 0f;
            }
        }
    }

    private void CalculateOfflineProgress()
    {
        if (IdleTankManager.Instance == null) return;

        // If this is the player's very first time opening the idle game, bail out.
        if (!ES3.KeyExists(LastSaveTimeKey, IdleSaveFile)) return;

        // Get the times
        DateTime lastSaveTime = ES3.Load<DateTime>(LastSaveTimeKey, IdleSaveFile);
        TimeSpan timeAway = DateTime.UtcNow - lastSaveTime;

        // Math: Total seconds away * Tank's generation rate
        float secondsAway = (float)timeAway.TotalSeconds;
        float offlineEarned = secondsAway * IdleTankManager.Instance.GetTotalBoonsPerSecond();

        // Add to the tank, but enforce the capacity!
        float spaceLeftInTank = MaxTankCapacity - UncollectedBoons;
        float boonsActuallyAdded = Mathf.Min(offlineEarned, spaceLeftInTank);

        UncollectedBoons += boonsActuallyAdded;

        // Optional: Send this data to a UI Popup!
        if (boonsActuallyAdded > 0)
        {
            Debug.Log($"Welcome Back! You were gone for {timeAway.Hours}h {timeAway.Minutes}m. " +
                      $"Your shrunken Kaiju generated {boonsActuallyAdded:F0} Boons!");

            if (UncollectedBoons >= MaxTankCapacity)
            {
                Debug.Log("Your tank is completely full of Boons! Empty it to start generating more.");
            }
        }
    }

    // Called by a UI Button to "Empty the Tank"
    public void CollectTankBoons()
    {
        BankedBoons += UncollectedBoons;
        UncollectedBoons = 0f;

        //TRIGGER DISCRETE EVENT
        OnBankChanged?.Invoke(BankedBoons);

        // Immediately update the tank UI to show it's empty
        OnTankFilled?.Invoke(UncollectedBoons, MaxTankCapacity);

        SaveEconomy();
    }

    private void LoadEconomy()
    {
        BankedBoons = ES3.Load("Idle.BankedBoons", IdleSaveFile, 0f);
        UncollectedBoons = ES3.Load("Idle.UncollectedBoons", IdleSaveFile, 0f);
        MaxTankCapacity = ES3.Load("Idle.MaxTankCapacity", IdleSaveFile, 500f);
    }

    private void SaveEconomy()
    {
        ES3.Save("Idle.BankedBoons", BankedBoons, IdleSaveFile);
        ES3.Save("Idle.UncollectedBoons", UncollectedBoons, IdleSaveFile);
        ES3.Save("Idle.MaxTankCapacity", MaxTankCapacity, IdleSaveFile);
        ES3.Save(LastSaveTimeKey, DateTime.UtcNow, IdleSaveFile);
    }

    // Unity lifecycle hooks to ensure we save when the player leaves
    private void OnApplicationQuit()
    {
        SaveEconomy();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveEconomy();
    }


}
