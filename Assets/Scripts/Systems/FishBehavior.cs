using UnityEngine;

/// <summary>
/// View/behaviour for an instantiated fish GameObject. Keep visual logic here and bind FishData at spawn.
/// </summary>
[DisallowMultipleComponent]
public class FishBehaviour : MonoBehaviour
{
    public FishData Data { get; private set; }

    public void Assign(FishData data)
    {
        Data = data;
        ApplyVisuals();
    }

    private void ApplyVisuals()
    {
        // Minimal example: name the GameObject for debugging.
        if (Data != null)
            gameObject.name = $"Fish_{Data.Id}_{Data.Name}";
    }

    // Example hook for future movement/AI code
    void Update()
    {
        // movement/animation logic goes here, using Data.Speed / Data.Weight etc.
    }
}