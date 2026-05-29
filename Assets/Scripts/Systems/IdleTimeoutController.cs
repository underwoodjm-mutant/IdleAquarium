using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class IdleTimeoutController : MonoBehaviour
{
    [Header("Idle Settings")]
    [Tooltip("Time in seconds of no mouse movement before the UI fades out.")]
    [SerializeField] private float timeUntilFade = 5f;

    [Tooltip("How fast the UI fades in and out. Higher is faster.")]
    [SerializeField] private float fadeSpeed = 3f;

    private CanvasGroup _canvasGroup;
    private Vector3 _lastMousePosition;
    private float _idleTimer = 0f;
    private bool _isHidden = false;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        // RequireComponent ensures this will never be null
        _canvasGroup = GetComponent<CanvasGroup>();
        _lastMousePosition = Input.mousePosition;
    }

    private void Update()
    {
        //Detect if the mouse moved or any key was pressed
        if (Input.mousePosition != _lastMousePosition || Input.anyKeyDown)
        {
            ResetTimerAndWakeUp();
        }
        else
        {
            //If no movement, tick the timer up
            _idleTimer += Time.deltaTime;

            if (_idleTimer >= timeUntilFade && !_isHidden)
            {
                FadeOut();
            }
        }

        //Update the tracking position for the next frame
        _lastMousePosition = Input.mousePosition;
    }

    private void ResetTimerAndWakeUp()
    {
        _idleTimer = 0f;

        if (_isHidden)
        {
            FadeIn();
        }
    }

    private void FadeOut()
    {
        _isHidden = true;
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeToAlpha(0f));
    }

    private void FadeIn()
    {
        _isHidden = false;
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeToAlpha(1f));
    }

    private IEnumerator FadeToAlpha(float targetAlpha)
    {
        // Smoothly transition the alpha until it reaches the target
        while (!Mathf.Approximately(_canvasGroup.alpha, targetAlpha))
        {
            // MoveTowards is perfectly linear and clean for UI fading
            _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);

            // CRITICAL: Prevent clicking on invisible buttons!
            // If the UI is mostly faded out, turn off interactability and raycast blocking.
            bool isVisible = _canvasGroup.alpha > 0.1f;
            _canvasGroup.interactable = isVisible;
            _canvasGroup.blocksRaycasts = isVisible;

            yield return null;
        }
    }
}
