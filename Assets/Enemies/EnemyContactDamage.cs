using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyContactDamage : MonoBehaviour
{
    public int contactDamage = 1;
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
        var dmg = other.GetComponent<IDamageable>();
        if (dmg != null)
        {
            //dmg.TakeDamage(contactDamage, this);
        }
    }
}