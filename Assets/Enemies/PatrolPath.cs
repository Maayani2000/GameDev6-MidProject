using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolPath : MonoBehaviour
{
    public Transform[] waypoints;

    public void GatherWaypointsFromChildren()
    {
        waypoints = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++) waypoints[i] = transform.GetChild(i);
    }
}
