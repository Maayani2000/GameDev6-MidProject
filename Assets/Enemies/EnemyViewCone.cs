using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyViewCone : MonoBehaviour
{
    public float viewDistance = 6f; // how far
    public float arcAngle = 60f; // how wide
    public LayerMask partyLayer;
    public LayerMask wallsANDfurnitureLayer;
    public LayerMask bigObjectsLayer;
    public Transform eyeOrigin; // where eyes located
    public float sampleStep = 0.25f; // for blocked 


    public Transform GetVisibleTarget()
    {
        Vector2 origin = eyeOrigin != null ? (Vector2)eyeOrigin.position : (Vector2)transform.position;
        Vector2 forward = eyeOrigin != null ? (Vector2)eyeOrigin.right : (Vector2)transform.right;

        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, viewDistance, partyLayer);
        if (hits == null || hits.Length == 0) return null;

        foreach (var h in hits)
        {
            if (h == null) continue;
            Vector2 toTarget = ((Vector2)h.transform.position - origin).normalized;
            float angle = Vector2.Angle(forward, toTarget);
            if (angle > arcAngle * 0.5f) continue;

            if (!IsLineBlocked(origin, (Vector2)h.transform.position, wallsANDfurnitureLayer | bigObjectsLayer, sampleStep)) // check line of sight
                return h.transform;
        }
        return null;
    }

    bool IsLineBlocked(Vector2 a, Vector2 b, LayerMask obstacleMask, float step)
    {
        float dist = Vector2.Distance(a, b);
        int steps = Mathf.Max(1, Mathf.CeilToInt(dist / step));
        for (int i = 1; i <= steps; i++)
        {
            Vector2 samplePoint = Vector2.Lerp(a, b, i / (float)steps);
            if (Physics2D.OverlapPoint(samplePoint, obstacleMask) != null)
                return true;
        }
        return false;
    }

    public bool CanSee(Transform target)
    {
        if (target == null) return false;
        Vector2 origin = eyeOrigin != null ? (Vector2)eyeOrigin.position : (Vector2)transform.position;
        Vector2 toTarget = (Vector2)target.position - origin;
        if (toTarget.sqrMagnitude > viewDistance * viewDistance) return false;
        float angle = Vector2.Angle((eyeOrigin != null ? (Vector2)eyeOrigin.right : (Vector2)transform.right), toTarget.normalized);
        if (angle > arcAngle * 0.5f) return false;
        RaycastHit2D hit = Physics2D.Raycast(origin, toTarget.normalized, toTarget.magnitude, wallsANDfurnitureLayer | partyLayer); // point check
        if (hit.collider == null) return true;
        return hit.collider.transform == target;
    }

    void OnDrawGizmosSelected()
    {
        if (eyeOrigin == null) return;
        Vector2 origin = eyeOrigin.position;
        Vector2 forward = eyeOrigin.right;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, viewDistance);
        Vector2 left = Quaternion.Euler(0, 0, -arcAngle * 0.5f) * forward;
        Vector2 right = Quaternion.Euler(0, 0, arcAngle * 0.5f) * forward;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin, origin + left * viewDistance);
        Gizmos.DrawLine(origin, origin + right * viewDistance);
    }
}