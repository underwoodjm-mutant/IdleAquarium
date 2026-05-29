using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BankUIController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The TextMeshPro element that displays banked Lumins.")]
    [SerializeField] private TextMeshProUGUI bankText;

    [Header("Animation Settings")]
    [Tooltip("How fast the numbers roll up to the target amount. Higher is faster.")]
    [SerializeField] private float countSpeed = 1.2f;

    // We track the visual number separately from the real EconomyManager data
    private float _currentDisplayedBoons = 0f;
    private float _targetBoons = 0f;
    private Coroutine _countCoroutine;

    private void OnEnable()
    {
        // Subscribe to the EconomyManager's broadcast
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnBankChanged += OnBankDataUpdated;
        }
    }

    private void OnDisable()
    {
        // Always unsubscribe to prevent memory leaks when the UI is hidden!
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnBankChanged -= OnBankDataUpdated;
        }
    }

    private void Start()
    {
        // On boot, snap the UI instantly to whatever the save file loaded
        if (EconomyManager.Instance != null)
        {
            _currentDisplayedBoons = EconomyManager.Instance.BankedBoons;
            _targetBoons = _currentDisplayedBoons;
            UpdateTextDisplay();
        }
    }

    /// <summary>
    /// Triggered whenever the EconomyManager announces a change in funds.
    /// </summary>
    private void OnBankDataUpdated(float newTotal)
    {
        _targetBoons = newTotal;

        // If an animation is already running (e.g., they clicked twice fast), stop it
        if (_countCoroutine != null)
        {
            StopCoroutine(_countCoroutine);
        }

        // Start the rolling animation toward the new total
        _countCoroutine = StartCoroutine(CountUpToTarget());
    }

    private IEnumerator CountUpToTarget()
    {
        // Keep looping until the visual number reaches the real backend target
        while (!Mathf.Approximately(_currentDisplayedBoons, _targetBoons))
        {
            // Smoothly interpolate the display number toward the target
            _currentDisplayedBoons = Mathf.Lerp(_currentDisplayedBoons, _targetBoons, Time.deltaTime * countSpeed);

            // Once it gets very close, snap it to prevent infinite microscopic decimals
            if (Mathf.Abs(_targetBoons - _currentDisplayedBoons) < 1f)
            {
                _currentDisplayedBoons = _targetBoons;
            }

            UpdateTextDisplay();

            // Wait until the next frame before continuing the loop
            yield return null;
        }
    }

    private void UpdateTextDisplay()
    {
        // Floor the value so the player only sees whole, clean numbers
        bankText.text = Mathf.FloorToInt(_currentDisplayedBoons).ToString("N0");
    }
}
