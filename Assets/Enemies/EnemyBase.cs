using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    public int contactDamage = 1;
    public float contactCooldown = 0.5f;

    public int maxHP = 3;
    int currentHP;
    int freezeHits = 0;

    [Header("Freeze")]
    public int freezeHitsToDie = 3;
    public bool frozen = false;

    bool isDead = false;

    [Header("References")]
    public Animator animator;
    public Collider2D hitCollider;

    void Awake()
    {
        currentHP = maxHP;
        if (hitCollider == null) hitCollider = GetComponent<Collider2D>();
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
        animator?.SetBool("Frozen", true);
        CancelInvoke(nameof(Enable));
        Invoke(nameof(Enable), seconds);
    }

    void Enable()
    {
        frozen = false;
        animator?.SetBool("Frozen", false);
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

    void OnTriggerEnter2D(Collider2D other)
    {
        //
    }
}
