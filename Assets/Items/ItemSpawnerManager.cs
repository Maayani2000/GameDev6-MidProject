using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawnerManager : MonoBehaviour
{
    public enum SpawnType { Keycard, ShipPart } // set point to key or ship parts

    [System.Serializable]
    public class SpawnPoint
    {
        public Transform point;
        public SpawnType type;
    }

    [Header("Spawn Points List")]
    public List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    [Header("Prefabs List")]
    public List<GameObject> keycardPrefabs; // neeed to attach all 3 different keycards prefabs (easier to index for doors than full random)
    public List<GameObject> shipPartPrefabs; // smae for all 4 different ship parts

    void Start()
    {
        List<GameObject> keyPool = new List<GameObject>(keycardPrefabs);
        List<GameObject> partPool = new List<GameObject>(shipPartPrefabs);

        foreach (var sp in spawnPoints) // cheking list for unused prefabs to avoid duplicates
        {
            if (sp.point == null) continue;

            GameObject prefab = null;

            if (sp.type == SpawnType.Keycard && keyPool.Count > 0)
            {
                int r = Random.Range(0, keyPool.Count);
                prefab = keyPool[r];
                keyPool.RemoveAt(r);
            }
            else if (sp.type == SpawnType.ShipPart && partPool.Count > 0)
            {
                int r = Random.Range(0, partPool.Count);
                prefab = partPool[r];
                partPool.RemoveAt(r);
            }

            if (prefab != null)
                Instantiate(prefab, sp.point.position, Quaternion.identity);
        }
    }
}
