using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Freezer : PlayableCharacter
{
    [Header("Freeze Ability")]
    public float freezeRadius = 3f;
    public float freezeDuration = 2.5f;
    public float cooldown = 4f;
    public Material outlineBlue;

    private bool isOnCooldown = false;

    protected override void InteractWith(Collider2D target) { }

    public override void SpecialAbility()
    {
        if (isOnCooldown) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, freezeRadius);
        var nearest = GetNearestInteractable(hits);
        if (nearest != null)
        {
            StartCoroutine(FreezeTarget(nearest.gameObject));
        }
    }

    private IEnumerator FreezeTarget(GameObject target)
    {
        isOnCooldown = true;

        var sr = target.GetComponent<SpriteRenderer>();
        Material originalMat = null;
        if (sr != null)
        {
            originalMat = sr.material;
            sr.material = outlineBlue;
        }

        var rb = target.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        yield return new WaitForSeconds(freezeDuration);

        if (rb != null) rb.simulated = true;
        if (sr != null && originalMat != null) sr.material = originalMat;

        yield return new WaitForSeconds(cooldown);
        isOnCooldown = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, freezeRadius);
    }

}
