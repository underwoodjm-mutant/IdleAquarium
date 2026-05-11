using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class BoolEvent : ScriptableObject
{
#if UNITY_EDITOR
    [Multiline]
    public string DeveloperDescription = "";
#endif
    [SerializeField]
    private bool triggered;

    public bool Triggered { get => triggered; set => triggered = false; }

    /// <summary>
    /// Triggers an event. Start the coroutine End Event immediately after
    /// </summary>
    public void TriggerEvent()
    {
        //Debug.Log("Event Triggered");
        triggered = true;
    }

    /// <summary>
    /// Waits for a single full frame then disables the trigger
    /// </summary>
    /// <returns></returns>
    public IEnumerator EndEvent()
    {
        float frameCount = Time.frameCount;

        while (Time.frameCount <= frameCount)
        {
            yield return null;
        }

        //Debug.Log("Event Disabled");
        triggered = false;

        yield return null;
    }

    /*
    /// <summary>
    /// Waits for the time to given to end the event
    /// </summary>
    /// <param name="endDelay"></param>
    /// <returns></returns>
    public IEnumerator EndEvent(float endDelay)
    {
        float frameCount = Time.frameCount;

        while (Time.frameCount <= frameCount)
        {
            yield return null;
        }

        yield return new WaitForSeconds(endDelay);
        triggered = false;

        yield return null;
    }
    */
}
