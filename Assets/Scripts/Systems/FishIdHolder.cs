using UnityEngine;

// Attach to fish prefabs to provide an explicit ID mapping (preferred)
public class FishIdHolder : MonoBehaviour
{
    [Tooltip("Unique fish ID that matches FishDatabase / CSV ID")]
    public int Id;
}
