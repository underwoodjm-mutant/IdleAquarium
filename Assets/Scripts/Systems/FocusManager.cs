using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FocusManager : Singleton<FocusManager>
{
    [Header("Focus Settings")]
    [Tooltip("The multiplier applied to Lumin generation while Focus Mode is active.")]
    public float luminMultiplier = 1.5f;

    // --- State Properties ---
    public bool IsFocusModeActive { get; private set; }
    public float TimeRemaining { get; private set; }

    // The EconomyManager will read this property to apply the boost
    public float CurrentMultiplier => IsFocusModeActive ? luminMultiplier : 1.0f;

    // --- UI Events ---
    public event Action OnFocusStarted;
    public event Action<float> OnFocusTick; // Passes remaining seconds for UI text
    public event Action OnFocusEnded;

    private void Update()
    {
        if (!IsFocusModeActive) return;

        TimeRemaining -= Time.deltaTime;

        // Fire tick event every frame so the UI timer can update smoothly
        OnFocusTick?.Invoke(TimeRemaining);

        if (TimeRemaining <= 0)
        {
            CompleteFocusMode();
        }
    }

    /// <summary>
    /// Starts the Pomodoro timer and locks out the main game UI.
    /// </summary>
    /// <param name="durationInMinutes">Time to focus, usually 25 or 50 minutes.</param>
    public void StartFocusMode(float durationInMinutes)
    {
        if (IsFocusModeActive) return;

        TimeRemaining = durationInMinutes * 60f; // Convert to seconds
        IsFocusModeActive = true;

        Debug.Log($"[FocusManager] Focus Mode started for {durationInMinutes} minutes. Lumin generation is now {luminMultiplier}x.");

        OnFocusStarted?.Invoke();
    }

    /// <summary>
    /// Ends the timer prematurely. The player regains UI access but loses the boost.
    /// </summary>
    public void CancelFocusMode()
    {
        if (!IsFocusModeActive) return;

        IsFocusModeActive = false;
        TimeRemaining = 0;

        Debug.Log("[FocusManager] Focus Mode canceled early. Multiplier lost.");

        OnFocusEnded?.Invoke();
    }

    /// <summary>
    /// Handles the natural completion of the timer.
    /// </summary>
    private void CompleteFocusMode()
    {
        IsFocusModeActive = false;
        TimeRemaining = 0;

        // Trigger your gentle chime or UI notification here
        // SoundManager.Instance.PlaySFX(SoundType.FocusCompleteChime);

        Debug.Log("[FocusManager] Focus session complete! Commencing break time.");

        OnFocusEnded?.Invoke();
    }
}
