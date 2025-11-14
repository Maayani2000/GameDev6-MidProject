using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    [Min(0)]
    public int contactDamage = 25;
    public float contactCooldown = 0.5f;

    public int maxHP = 3;
    int currentHP;
    int freezeHits = 0;

    [Header("Freeze")]
    public int freezeHitsToDie = 3;
    public bool frozen = false;

    bool isDead = false;
    Rigidbody2D rb;
    EnemyAI enemyAI;
    PatrolMover patrolMover;

    [Header("References")]
    public Animator animator;
    public Collider2D hitCollider;

    [Header("Visuals")]
    public SpriteRenderer sr;
    public Color normalColor = Color.white;
    public Color frozenColor = Color.cyan;

    void Awake()
    {
        currentHP = maxHP;
        if (hitCollider == null) hitCollider = GetComponent<Collider2D>();
        if (animator == null) animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        enemyAI = GetComponent<EnemyAI>();
        patrolMover = GetComponent<PatrolMover>();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;
        currentHP -= amount;
        animator?.SetTrigger("Hit");
        if (currentHP <= 0)
            Die();
    }

    public void TakeFreezeHit() // need to be called by Freezer's ability
    {
        if (isDead) return;
        freezeHits++;
        animator?.SetTrigger("FreezeHit");
        if (freezeHits >= freezeHitsToDie)
            Die();
    }

    public void DisableTemporarily(float seconds)
    {
        if (isDead) return;
        frozen = true;
        SetFrozenAnimator(true);
        if (enemyAI != null) enemyAI.enabled = false;
        if (patrolMover != null) patrolMover.enabled = false;
        if (rb != null) rb.velocity = Vector2.zero;
        CancelInvoke(nameof(Enable));
        Invoke(nameof(Enable), seconds);
    }

    void Enable()
    {
        frozen = false;
        SetFrozenAnimator(false);
        if (enemyAI != null) enemyAI.enabled = true;
        if (patrolMover != null) patrolMover.enabled = true;
        if (sr != null)
                sr.color = normalColor; 
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        animator?.SetTrigger("Die");

        if (hitCollider != null) hitCollider.enabled = false; // disable colliders
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false; // disable physics
        Destroy(gameObject, 1.5f);
    }

    void SetFrozenAnimator(bool value)
    {
        if (animator == null) return;
        try
        {
            animator.SetBool("Frozen", value);
            if (sr != null)
                sr.color = frozenColor;
        }
        catch (MissingComponentException)
        {
            //Debug.LogWarning($"EnemyBase on {name} tried to set Animator 'Frozen' but no Animator exists.");
            animator = null;
        }
    }

    float lastContactTime = Mathf.NegativeInfinity;

    void OnCollisionEnter2D(Collision2D collision)
    {
        TryDealContactDamage(collision.collider);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        TryDealContactDamage(collision.collider);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryDealContactDamage(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        TryDealContactDamage(other);
    }

    void TryDealContactDamage(Collider2D other)
    {
        if (contactDamage <= 0) return;
        if (Time.time - lastContactTime < contactCooldown) return;
        if (other == null) return;

        var damageable = other.GetComponent<IDamageable>() ?? other.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(contactDamage);
            lastContactTime = Time.time;
        }
    }
}
