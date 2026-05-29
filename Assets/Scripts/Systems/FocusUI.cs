using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FocusUI : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI timerText;

    private void OnEnable()
    {
        FocusManager.Instance.OnFocusTick += UpdateTimerText;
    }

    private void OnDisable()
    {
        if (FocusManager.Instance != null)
        {
            FocusManager.Instance.OnFocusTick -= UpdateTimerText;
        }
    }

    private void UpdateTimerText(float secondsRemaining)
    {
        // Format the float into a clean MM:SS string
        TimeSpan time = TimeSpan.FromSeconds(secondsRemaining);
        timerText.text = time.ToString(@"mm\:ss");
    }
}
