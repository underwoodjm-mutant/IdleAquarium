using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class ConfirmationPopupController : Singleton<ConfirmationPopupController>
{
    [Header("UI References")]
    [SerializeField] private GameObject popupPanel; // The visual window to turn on/off
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("Buttons")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI confirmButtonText; // To change text dynamically

    [SerializeField] private Button cancelButton;
    [SerializeField] private TextMeshProUGUI cancelButtonText;  // To change text dynamically

    // Store the actions so we can call them when buttons are clicked
    private Action _onConfirmAction;
    private Action _onCancelAction;

    private void Start()
    {
        // Ensure the popup starts completely hidden
        popupPanel.SetActive(false);

        // Wire up the physical UI buttons to our internal handler methods
        confirmButton.onClick.AddListener(HandleConfirmClicked);
        cancelButton.onClick.AddListener(HandleCancelClicked);
    }

    /// <summary>
    /// Opens the popup and waits for user input.
    /// </summary>
    /// <param name="title">The header text.</param>
    /// <param name="message">The warning or description.</param>
    /// <param name="onConfirm">The function to run if they say Yes.</param>
    /// <param name="onCancel">Optional: The function to run if they say No.</param>
    /// <param name="confirmTxt">Optional: Custom text for the Yes button.</param>
    /// <param name="cancelTxt">Optional: Custom text for the No button.</param>
    public void ShowPopup(string title, string message, Action onConfirm, Action onCancel = null, string confirmTxt = "Confirm", string cancelTxt = "Cancel")
    {
        // 1. Set the visual text
        titleText.text = title;
        messageText.text = message;
        confirmButtonText.text = confirmTxt;
        cancelButtonText.text = cancelTxt;

        // 2. Store the passed-in functions
        _onConfirmAction = onConfirm;
        _onCancelAction = onCancel;

        // 3. Show the panel
        popupPanel.SetActive(true);
    }

    private void HandleConfirmClicked()
    {
        // Hide the popup first to prevent double-clicks
        popupPanel.SetActive(false);

        // Execute the stored confirmation method
        _onConfirmAction?.Invoke();
    }

    private void HandleCancelClicked()
    {
        popupPanel.SetActive(false);

        // Execute the stored cancel method (if one was provided)
        _onCancelAction?.Invoke();
    }
}
