using System.Collections;
using System.Collections.Generic;
//using System.Drawing.Design;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "NewFloatVariable", menuName = "Variable/Float", order = 25)]
public class FloatVariable : ScriptableObject
{
#if UNITY_EDITOR
    [Multiline]
    public string DeveloperDescription = "";
#endif
    public float Value;
    
    [Header("Clamp")] 
    [SerializeField] private bool clampToRange = false;
    [SerializeField] private Vector2 minMax;

    public void SetValue(float value)
    {
        Value = !clampToRange ? value : Clamp(value, minMax.x, minMax.y);
    }

    public void SetValue(FloatVariable value)
    {
        Value = !clampToRange ? value.Value : Clamp(value.Value, minMax.x, minMax.y);
    }

    public void ApplyChange(float amount)
    {
        if (!clampToRange) Value += amount;
        else Value = Clamp(Value + amount, minMax.x, minMax.y);
    }

    public void ApplyChange(FloatVariable amount)
    {
        if (!clampToRange) Value += amount.Value;
        else Value = Clamp(Value + amount.Value, minMax.x, minMax.y);
    }

    private float Clamp(float value, float min, float max)
    {
        if (value < min) value = min;
        if (value > max) value = max;
        return value;
    }
}
