using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyContactDamage : MonoBehaviour
{
    [Min(0)]
    public int contactDamage = 25;
    public bool damageOnEnter = true;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!damageOnEnter) return;
        TryApplyDamage(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!damageOnEnter) return;
        TryApplyDamage(other);
    }

    private void TryApplyDamage(Collider2D other)
    {
        if (contactDamage <= 0) return;

        var damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(contactDamage);
        }
    }
}