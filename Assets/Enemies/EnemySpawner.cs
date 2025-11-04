using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SpawnPoint
{
    public Transform spawnTransform;
    public GameObject enemyPrefab;
    public bool spawnOnStart;
    public bool patrolAtSpawn; // if true assign PatrolPath / roam flags after spawn
    public PatrolPath patrolPath; // optional
    public bool roam; // if true set roam mode
}

public class EnemySpawner : MonoBehaviour
{
    public SpawnPoint[] spawnPoints;

    void Start()
    {
        foreach (var sp in spawnPoints)
        {
            if (sp.spawnOnStart && sp.enemyPrefab != null)
            {
                SpawnAt(sp);
            }
        }
    }

    public GameObject SpawnAt(SpawnPoint sp)
    {
        var go = Instantiate(sp.enemyPrefab, sp.spawnTransform.position, sp.spawnTransform.rotation, transform);
        var mover = go.GetComponent<PatrolMover>();
        if (mover != null)
        {
            if (sp.roam)
            {
                mover.mode = PatrolMover.Mode.RoamArea;
                mover.roamCenter = sp.spawnTransform.position;
            }
            else
            {
                mover.mode = PatrolMover.Mode.WaypointPath;
                if (sp.patrolPath != null) mover.path = sp.patrolPath;
            }
        }
        return go;
    }
}
