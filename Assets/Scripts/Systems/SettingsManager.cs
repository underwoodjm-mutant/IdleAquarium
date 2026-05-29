using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Audio;

public class SettingsManager : Singleton<SettingsManager>
{
    [Header("UI Canvas References")]
    [SerializeField] private GameObject settingsCanvasObject;

    [Header("UI Controls - Window")]
    [SerializeField] private Toggle alwaysOnTopToggle;
    [SerializeField] private Slider windowScaleSlider;
    [SerializeField] private Toggle fullScreenToggle;

    [Header("UI Controls - Audio")]
    [SerializeField] private AudioMixer mainAudioMixer;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider ambientVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle muteWhenUnfocusedToggle;

    private bool _muteWhenUnfocused = true;
    private bool _isMutedBySystem = false;
    private float _currentWindowScale = 1.0f;

    private InputAction _toggleSettingsAction;

    // Save File Constants
    private const string SettingsSaveFile = "IdleSettingsData.es3";

    private void Awake()
    {
        base.Awake();

        //Initialize and bind the Escape key to our action
        _toggleSettingsAction = new InputAction(
            name: "ToggleSettings",
            type: InputActionType.Button,
            binding: "<Keyboard>/escape"
        );

        //Hook up the callback method when the key is pressed (performed)
        _toggleSettingsAction.performed += OnEscapePressed;
    }

    private void OnEnable()
    {
        //Input actions must be explicitly enabled to start listening
        _toggleSettingsAction.Enable();
    }

    private void OnDisable()
    {
        //Clean up to prevent memory leaks
        _toggleSettingsAction.Disable();
    }

    private void Start()
    {
        LoadSettings();

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.minValue = 0.0001f;
            masterVolumeSlider.maxValue = 1f;
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        // Hook up UI listeners dynamically
        alwaysOnTopToggle.onValueChanged.AddListener(SetAlwaysOnTop);
        windowScaleSlider.onValueChanged.AddListener(SetWindowScale);
        ambientVolumeSlider.onValueChanged.AddListener(SetAmbientVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);

        if (muteWhenUnfocusedToggle != null)
        {
            muteWhenUnfocusedToggle.onValueChanged.AddListener(OnMuteInBackgroundToggled);
        }

        if (fullScreenToggle != null)
        {
            fullScreenToggle.onValueChanged.AddListener(SetFullscreen);
        }
    }

    /// <summary>
    /// Event callback fired by the New Input System when Escape is hit.
    /// </summary>
    private void OnEscapePressed(InputAction.CallbackContext context)
    {
        // Check if Focus Mode (Pomodoro) is blocking menu access
        if (FocusManager.Instance != null && FocusManager.Instance.IsFocusModeActive)
        {
            return;
        }

        ToggleSettingsMenu();
    }

    public void ToggleSettingsMenu()
    {
        if (settingsCanvasObject != null)
        {
            bool currentState = settingsCanvasObject.activeSelf;
            settingsCanvasObject.SetActive(!currentState);
            Debug.Log($"[Settings] Menu toggled to: {!currentState}");
        }
    }

    // ==========================================
    // --- APP / WINDOW FUNCTIONS ---
    // ==========================================

    public void SetAlwaysOnTop(bool value)
    {
        //Save the preference to Easy Save 3
        ES3.Save("Settings.AlwaysOnTop", value, SettingsSaveFile);

        //Tell the OS to physically move the window's Z-order
        if (TransparentWindow.Instance != null)
        {
            TransparentWindow.Instance.SetAlwaysOnTopState(value);
        }

        Debug.Log($"[Settings] Always On Top set to: {value}");
    }

    /// <summary>
    /// Scales the physical OS window based on a base resolution.
    /// Snaps the slider input to quarter-steps (0.5, 0.75, 1.0) to prevent OS resizing lag.
    /// </summary>
    private void SetWindowScale(float rawScaleFactor)
    {
        // 1. Snap the scale to clean 0.25 intervals 
        float snappedScale = Mathf.Round(rawScaleFactor * 4f) / 4f;

        // 2. If the drag hasn't actually crossed a new 0.25 threshold, bail out immediately!
        if (Mathf.Approximately(_currentWindowScale, snappedScale)) return;

        _currentWindowScale = snappedScale;

        // 3. Define your "1.0x" base resolution (e.g., 800x600 for a compact widget)
        int baseWidth = 800;
        int baseHeight = 600;

        int targetWidth = Mathf.RoundToInt(baseWidth * snappedScale);
        int targetHeight = Mathf.RoundToInt(baseHeight * snappedScale);

        // 4. Command Unity to resize the OS window
        Screen.SetResolution(targetWidth, targetHeight, FullScreenMode.Windowed);

        // 5. Save the preference
        ES3.Save("Settings.WindowScale", snappedScale, SettingsSaveFile);

        Debug.Log($"[Settings] Window scaled to: {snappedScale}x ({targetWidth}x{targetHeight})");
    }

    /// <summary>
    /// Toggles between a transparent desktop widget and an opaque fullscreen screensaver.
    /// </summary>
    private void SetFullscreen(bool isFullscreen)
    {
        if (isFullscreen)
        {
            // 1. Get the monitor's native resolution
            Resolution nativeRes = Screen.resolutions[Screen.resolutions.Length - 1];

            // 2. Snap to borderless fullscreen
            Screen.SetResolution(nativeRes.width, nativeRes.height, FullScreenMode.FullScreenWindow);

            // 3. Make the camera background fully opaque (Solid Black) so it covers the desktop
            Camera.main.backgroundColor = new Color(0, 0, 0, 1f);
        }
        else
        {
            // 1. Revert to standard windowed mode using the player's saved scale
            // We temporarily reset _currentWindowScale so the function doesn't bail out early
            float savedScale = _currentWindowScale;
            _currentWindowScale = 0f;
            SetWindowScale(savedScale);

            // 2. Restore camera transparency for the widget mode
            Camera.main.backgroundColor = new Color(0, 0, 0, 0f);
        }

        // Save the preference
        ES3.Save("Settings.Fullscreen", isFullscreen, SettingsSaveFile);

        Debug.Log($"[Settings] Fullscreen set to: {isFullscreen}");
    }

    // ==========================================
    // --- APPLICATION CONTROL ---
    // ==========================================

    /// <summary>
    /// Unity lifecycle method called automatically when the application window gains or loses OS focus.
    /// </summary>
    private void OnApplicationFocus(bool hasFocus)
    {
        if (_muteWhenUnfocused)
        {
            if (!hasFocus)
            {
                // Mute audio when clicking away to another app
                mainAudioMixer.SetFloat("MasterVolume", -80f);
                _isMutedBySystem = true;
            }
            else if (_isMutedBySystem)
            {
                // Restore audio based on the slider value when clicking back
                SetMasterVolume(masterVolumeSlider.value);
                _isMutedBySystem = false;
            }
        }
    }

    private void OnMuteInBackgroundToggled(bool isOn)
    {
        _muteWhenUnfocused = isOn;
        SaveSettings();
    }

    /// <summary>
    /// Saves all outstanding settings and cleanly exits the companion application.
    /// Hook this up to your UI "Quit Game" button's OnClick() event.
    /// </summary>
    public void QuitApplication()
    {
        Debug.Log("[Settings] Initiating clean application shutdown...");

        // Force Easy Save 3 to write its buffer to the actual file on disk.
        // This guarantees no save corruption or lost progress if the player clicks quit right after earning Lumins!
        try
        {
            ES3.StoreCachedFile();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Settings] Failed to flush ES3 cache before quitting: {e.Message}");
        }

        //  Shut down the application
#if UNITY_EDITOR
        // If testing inside the Unity Editor, stop the play state
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // If running the built standalone .exe, close the operating system process
        Application.Quit();
#endif
    }

    // ==========================================
    // --- AUDIO FUNCTIONS ---
    // ==========================================

    public void SetAmbientVolume(float value)
    {
        // Muffle or raise background ocean machinery hums
        ES3.Save("Settings.AmbientVolume", value, SettingsSaveFile);
        // SoundManager.Instance.SetAmbientVolume(value);
    }

    public void SetSfxVolume(float value)
    {
        // Control mechanical UI clicks and staple gun sounds
        ES3.Save("Settings.SfxVolume", value, SettingsSaveFile);
        // SoundManager.Instance.SetSfxVolume(value);
    }

    private void SetMasterVolume(float sliderValue)
    {
        // Convert the 0.0001 - 1 slider to logarithmic decibels
        float decibels = Mathf.Log10(sliderValue) * 20;
        if (sliderValue <= 0.0001f)
        {
            decibels = -80f; // Effectively silent
        }
        mainAudioMixer.SetFloat("MasterVolume", decibels);
        SaveSettings();
    }

    // ==========================================
    // --- DATA LOADING ---
    // ==========================================

    private void LoadSettings()
    {
        // Load with safe default fallbacks
        bool alwaysOnTop = ES3.Load("Settings.AlwaysOnTop", SettingsSaveFile, true);
        float windowScale = ES3.Load("Settings.WindowScale", SettingsSaveFile, 1.0f);
        float ambientVol = ES3.Load("Settings.AmbientVolume", SettingsSaveFile, 0.75f);
        float sfxVol = ES3.Load("Settings.SfxVolume", SettingsSaveFile, 0.75f);

        // Apply to UI elements visually
        alwaysOnTopToggle.isOn = alwaysOnTop;
        windowScaleSlider.value = windowScale;
        ambientVolumeSlider.value = ambientVol;
        sfxVolumeSlider.value = sfxVol;

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = ES3.Load("MasterVolume", SettingsSaveFile, 0.75f);
            SetMasterVolume(masterVolumeSlider.value);
        }

        if (muteWhenUnfocusedToggle != null)
        {
            _muteWhenUnfocused = ES3.Load("MuteWhenUnfocused", SettingsSaveFile, true);
            muteWhenUnfocusedToggle.isOn = _muteWhenUnfocused;
        }

        if (fullScreenToggle != null)
        {
            bool isFullscreen = ES3.Load("Settings.Fullscreen", SettingsSaveFile, false);
            fullScreenToggle.isOn = isFullscreen;

            // Apply the visual state immediately on boot
            SetFullscreen(isFullscreen);
        }

        // Apply actual logic on startup
        SetWindowScale(windowScale);
    }

    private void SaveSettings()
    {
        ES3.Save("MasterVolume", masterVolumeSlider.value, SettingsSaveFile);
        ES3.Save("MuteWhenUnfocused", _muteWhenUnfocused, SettingsSaveFile);
    }
}
