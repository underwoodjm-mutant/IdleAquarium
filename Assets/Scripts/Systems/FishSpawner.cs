using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lightweight spawner with robust prefab->id mapping (FishIdHolder preferred).
/// Keeps legacy fallback to index-based mapping to avoid breaking existing setups.
/// </summary>
public class FishSpawner : MonoBehaviour
{
    [Tooltip("List of fish prefabs. Prefer prefabs with FishIdHolder component for deterministic mapping.")]
    public List<GameObject> fishPrefabs = new List<GameObject>();

    private Dictionary<int, GameObject> _prefabById;

    void Awake()
    {
        _prefabById = new Dictionary<int, GameObject>();
        for (int i = 0; i < fishPrefabs.Count; i++)
        {
            var prefab = fishPrefabs[i];
            if (prefab == null) continue;

            var idHolder = prefab.GetComponent<FishIdHolder>();
            if (idHolder != null && idHolder.Id > 0)
            {
                _prefabById[idHolder.Id] = prefab;
            }
            else
            {
                // legacy fallback: assume 1-based index matches fish ID
                _prefabById[i + 1] = prefab;
            }
        }
    }

    public GameObject SpawnById(int fishId, Vector3 position, Quaternion rotation)
    {
        if (!_prefabById.TryGetValue(fishId, out var prefab))
        {
            Debug.LogWarning($"FishSpawner: No prefab mapped for fish id {fishId}");
            return null;
        }

        var go = Instantiate(prefab, position, rotation);
        return go;
    }

    public GameObject SpawnById(int fishId, Vector3 position)
    {
        return SpawnById(fishId, position, Quaternion.identity);
    }
}
