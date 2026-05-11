using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class IdleUIManager : MonoBehaviour
{
    [Header("Top Bar: The Bank")]
    [SerializeField] private TextMeshProUGUI _bankedBoonsText;

    [Header("Bottom Bar: The Tank")]
    [SerializeField] private TextMeshProUGUI _tankCapacityText;
    [SerializeField] private Slider _tankCapacitySlider;
    [SerializeField] private Button _collectButton;

    private void Start()
    {
        if (_collectButton != null)
        {
            _collectButton.onClick.AddListener(OnCollectButtonPress);
        }

        // Force a UI refresh on startup so the numbers are right instantly
        RefreshInitialUI();
    }

    // EVENT SUBSCRIPTION
    // When this UI panel is turned on, it tunes into the radio broadcast.
    private void OnEnable()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnBankChanged += UpdateBankUI;
            EconomyManager.Instance.OnTankFilled += UpdateTankUI;
        }
    }

    // When this UI panel is turned off, it stops listening to prevent memory leaks.
    private void OnDisable()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnBankChanged -= UpdateBankUI;
            EconomyManager.Instance.OnTankFilled -= UpdateTankUI;
        }
    }

    private void RefreshInitialUI()
    {
        if (EconomyManager.Instance == null) return;
        UpdateBankUI(EconomyManager.Instance.BankedBoons);
        UpdateTankUI(EconomyManager.Instance.UncollectedBoons, EconomyManager.Instance.MaxTankCapacity);
    }

    // EVENT LISTENER METHODS 

    private void UpdateBankUI(float newBankAmount)
    {
        if (_bankedBoonsText != null)
        {
            _bankedBoonsText.text = $"Bank: {newBankAmount:F0}";
        }
    }

    private void UpdateTankUI(float currentUncollected, float currentMax)
    {
        if (_tankCapacityText != null)
        {
            _tankCapacityText.text = $"{currentUncollected:F0} / {currentMax:F0} Boons";
        }

        if (_tankCapacitySlider != null)
        {
            _tankCapacitySlider.value = currentUncollected / currentMax;
        }

        if (_collectButton != null)
        {
            _collectButton.interactable = currentUncollected > 0;
        }
    }

    private void OnCollectButtonPress()
    {
        EconomyManager.Instance.CollectTankBoons();
    }
}
