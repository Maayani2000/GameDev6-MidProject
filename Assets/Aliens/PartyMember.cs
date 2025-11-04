using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PartyMember : MonoBehaviour
{
    [Header("Follow Setting | per member)")]
    public float followForce = 40f;
    public float maxSpeed = 3f;
    public float nudgeStrenght = 1.6f;

    [HideInInspector] public LayerMask obstacleMask;

    Rigidbody2D rb;
    PartyManager manager;

    int indexOnTrail = 0; // 0 = leader 1 = followers
    bool leaderActive = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void FixedUpdate()
    {
        if (leaderActive) return; // leader controlled by PlayableCharacter

        if (manager == null) return;

        Vector2 target = manager.GetTrailPointFor(Mathf.Max(0, indexOnTrail - 1), this);
        MoveToward(target);
    }

    void MoveToward(Vector2 target)
    {
        Vector2 to = target - rb.position;
        Vector2 force = to.normalized * followForce * Time.fixedDeltaTime;
        rb.AddForce(force, ForceMode2D.Force);

        if (rb.velocity.sqrMagnitude > maxSpeed * maxSpeed)
            rb.velocity = rb.velocity.normalized * maxSpeed;
    }

    void OnCollisionEnter2D(Collision2D col) // push party members aside when colliding with them
    {
        int partyLayer = LayerMask.NameToLayer("partyLayer");
        if (col.gameObject.layer != partyLayer || gameObject.layer != partyLayer) return;

        var other = col.collider.GetComponent<PartyMember>();
        if (other == null || other == this) return;

        Vector2 avgNormal = Vector2.zero;
        foreach (var c in col.contacts) avgNormal += c.normal;
        avgNormal.Normalize();

        rb.AddForce(avgNormal * nudgeStrenght, ForceMode2D.Impulse);
    }

    #region PM Interface
    public void SetManager(PartyManager m) { manager = m; }

    public void SetIndex(int i) { indexOnTrail = Mathf.Max(0, i); }

    public void BecomeLeader(bool isLeader)
    {
        leaderActive = isLeader;
    }
    #endregion

    public void ApplySorting(int order)
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = order;
    }

    public bool IsPointBlocked(Vector2 point)
    {
        float r = 0.25f;
        return Physics2D.OverlapCircle(point, r, obstacleMask) != null;
    }

}