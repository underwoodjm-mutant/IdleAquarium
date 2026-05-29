using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FocusSetupController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider durationSlider;
    [SerializeField] private TextMeshProUGUI durationTextDisplay;
    [SerializeField] private Button startFocusButton;
    [SerializeField] private Button closeMenuButton;

    [Header("Canvas Swapping")]
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private GameObject focusCanvas;

    private void Start()
    {
        // Set up the slider bounds (e.g., 5 to 60 minutes)
        durationSlider.minValue = 5;
        durationSlider.maxValue = 60;
        durationSlider.wholeNumbers = true;
        durationSlider.value = 25; // Default Pomodoro length

        // Listeners
        durationSlider.onValueChanged.AddListener(UpdateDurationText);
        startFocusButton.onClick.AddListener(InitiateFocusMode);
        closeMenuButton.onClick.AddListener(CloseSetupMenu);

        // Initialize text
        UpdateDurationText(durationSlider.value);
    }

    private void UpdateDurationText(float value)
    {
        durationTextDisplay.text = $"{value} Minutes";
    }

    private void InitiateFocusMode()
    {
        //Tell the global manager to start ticking
        FocusManager.Instance.StartFocusMode(durationSlider.value);

        gameObject.SetActive(false);
    }   

    private void CloseSetupMenu()
    {
        gameObject.SetActive(false);
    }
}
