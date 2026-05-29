using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class FocusCanvasController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The text element displaying the ticking countdown.")]
    [SerializeField] private TextMeshProUGUI timerTextDisplay;

    [Tooltip("The emergency stop button to abort the session.")]
    [SerializeField] private Button cancelFocusButton;

    private void OnEnable()
    {
        //Subscribe to the tick event to update the clock every frame
        if (FocusManager.Instance != null)
        {
            FocusManager.Instance.OnFocusTick += UpdateTimerDisplay;
        }

        //Hook up the cancel button's click event dynamically
        if (cancelFocusButton != null)
        {
            cancelFocusButton.onClick.AddListener(OnCancelClicked);
        }
    }

    private void OnDisable()
    {
        // Always unsubscribe to prevent memory leaks when the Canvas turns off!
        if (FocusManager.Instance != null)
        {
            FocusManager.Instance.OnFocusTick -= UpdateTimerDisplay;
        }

        if (cancelFocusButton != null)
        {
            cancelFocusButton.onClick.RemoveListener(OnCancelClicked);
        }
    }

    /// <summary>
    /// Formats the raw seconds from the manager into a clean MM:SS clock.
    /// </summary>
    private void UpdateTimerDisplay(float remainingSeconds)
    {
        // Prevent the clock from showing negative numbers if there's a slight frame delay
        if (remainingSeconds < 0) remainingSeconds = 0;

        // Calculate minutes and seconds cleanly
        int minutes = Mathf.FloorToInt(remainingSeconds / 60F);
        int seconds = Mathf.FloorToInt(remainingSeconds % 60F);

        // Format the string to guarantee two digits (e.g., "05:09")
        timerTextDisplay.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    /// <summary>
    /// Intercepts the abort button and summons a warning popup before canceling.
    /// </summary>
    private void OnCancelClicked()
    {
        // Define what happens ONLY if the user confirms the abort
        Action confirmAbort = () =>
        {
            if (FocusManager.Instance != null)
            {
                FocusManager.Instance.CancelFocusMode();
            }
        };

        // Call the Singleton and pass the action through
        ConfirmationPopupController.Instance.ShowPopup(
            title: "ABORT PROTOCOL?",
            message: "Ending this session early will forfeit your 1.5x Lumin generation bonus. Are you sure?",
            onConfirm: confirmAbort,
            onCancel: null, // We don't need to do anything if they cancel the popup
            confirmTxt: "Abort Session",
            cancelTxt: "Keep Focusing"
        );
    }
}
