using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PatrolMover : MonoBehaviour
{
    public enum Mode { WaypointPath, RoamArea }
    public Mode mode = Mode.WaypointPath;

    [SerializeField] private Transform visionCone;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Waypoint settings")]
    public PatrolPath path;
    public float waypointTolerance = 0.2f;
    public bool loop = true;
    public float waitAtWaypoint = 0.5f;

    [Header("Roam settings")]
    public Vector2 roamCenter;
    public float roamRadius = 3f;
    public float roamPause = 1f;

    [Header("Movement")]
    public float moveSpeed = 2f;

    Rigidbody2D rb;
    int currentIndex = 0;
    Vector2 targetPos;
    bool paused = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (mode == Mode.RoamArea) roamCenter = transform.position;
        if (path == null && mode == Mode.WaypointPath)
        {
            path = GetComponentInParent<PatrolPath>(); // try to find path in parent
        }
    }
    void Start()
    {
        SetNextTarget();
    }

    void FixedUpdate()
    {
        if (paused) return;

        Vector2 pos = rb.position;
        Vector2 dir = (targetPos - pos);

        if (dir.sqrMagnitude <= waypointTolerance * waypointTolerance)
        {
            StartCoroutine(WaitAtPoint());
            return;
        }

        dir = dir.normalized;
        rb.MovePosition(pos + dir * moveSpeed * Time.fixedDeltaTime);

        if (dir.sqrMagnitude > 0.01f) // rotate only the vision cone (and not the sprite)
        {
            if (visionCone != null)
                visionCone.right = dir;


            if (spriteRenderer != null) // flip sprite horizontally based on movement
            {
                if (dir.x > 0)
                    spriteRenderer.flipX = false;
                else if (dir.x < 0)
                    spriteRenderer.flipX = true;
            }
        }
    }

    public void GoToPosition(Vector2 pos)
    {
        mode = Mode.WaypointPath;
        targetPos = pos;
        paused = false;
    }

    System.Collections.IEnumerator WaitAtPoint()
    {
        paused = true;
        yield return new WaitForSeconds(mode == Mode.WaypointPath ? waitAtWaypoint : roamPause);
        paused = false;
        SetNextTarget();
    }

    public void SetNextTarget()
    {
        if (mode == Mode.WaypointPath)
        {
            if (path == null || path.waypoints == null || path.waypoints.Length == 0)
            {
                targetPos = transform.position;
                return;
            }
            targetPos = path.waypoints[currentIndex].position;
            currentIndex++;
            if (currentIndex >= path.waypoints.Length) currentIndex = loop ? 0 : path.waypoints.Length - 1;
        }
        else // pick random point within roam radius
        {
            Vector2 rnd = Random.insideUnitCircle * roamRadius;
            targetPos = roamCenter + rnd;
        }
    }

    public void ResetTo(Vector2 position)
    {
        rb.position = position;
        SetNextTarget();
    }
}
