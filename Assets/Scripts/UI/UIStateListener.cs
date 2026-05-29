using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class UIStateListener : MonoBehaviour
{
    [Header("Behavior Settings")]
    [Tooltip("If true, this canvas turns ON during Focus Mode. If false, it turns OFF.")]
    [SerializeField] private bool isActiveDuringFocus;

    private Canvas _myCanvas;

    private void Awake()
    {
        _myCanvas = GetComponent<Canvas>();
    }

    private void OnEnable()
    {
        // Safety check to ensure we don't subscribe before the Singleton exists
        if (FocusManager.Instance != null)
        {
            FocusManager.Instance.OnFocusStarted += HandleFocusStateChange;
            FocusManager.Instance.OnFocusEnded += HandleFocusStateChange;
        }
    }

    private void Start()
    {
        // Late subscription for boot-up if OnEnable fired too early
        if (FocusManager.Instance != null)
        {
            FocusManager.Instance.OnFocusStarted -= HandleFocusStateChange; // Prevent double subscription
            FocusManager.Instance.OnFocusStarted += HandleFocusStateChange;

            FocusManager.Instance.OnFocusEnded -= HandleFocusStateChange;
            FocusManager.Instance.OnFocusEnded += HandleFocusStateChange;

            // Force an initial check on boot to make sure the Canvases start in the correct state!
            HandleFocusStateChange();
        }
        else
        {
            Debug.LogError($"UIStateListener on {gameObject.name} couldn't find FocusManager instance during Start. Make sure FocusManager is initialized before this listener.");
        }
    }

    private void OnDisable()
    {
        if (FocusManager.Instance != null)
        {
            FocusManager.Instance.OnFocusStarted -= HandleFocusStateChange;
            FocusManager.Instance.OnFocusEnded -= HandleFocusStateChange;
        }
    }

    private void HandleFocusStateChange()
    {
        // Check the current state of the manager
        bool isFocusing = FocusManager.Instance.IsFocusModeActive;

        // Compare it to this specific canvas's intended behavior
        if (isActiveDuringFocus)
        {
            _myCanvas.enabled = isFocusing;
        }
        else
        {
            _myCanvas.enabled = !isFocusing;
        }
    }
}
