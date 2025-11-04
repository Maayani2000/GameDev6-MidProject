using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class Telekinetic : PlayableCharacter
{
    [Header("Telekinesis")]
    public float pullRadius = 4f;
    public float focusDuration = 3f; // intead of cooldown

    private bool isUsingAbility = false;
    private GameObject focusedObject;

    protected override void InteractWith(Collider2D target)
    {
        //
    }

    public override void SpecialAbility()
    {
        if (isUsingAbility) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pullRadius, itemsLayer);
        var nearest = GetNearestInteractable(hits);
        if (nearest != null)
            StartCoroutine(TelekinesisRoutine(nearest.gameObject));
    }

    private IEnumerator TelekinesisRoutine(GameObject target)
    {
        isUsingAbility = true;
        SetControlsLocked(true);

        float t = 0f;
        while (t < focusDuration)
        {
            t += Time.deltaTime;

            target.transform.position += Vector3.up * Mathf.Sin(Time.time * 8f) * 0.002f; // a small floating motion
            yield return null;
        }

        SetControlsLocked(false);
        isUsingAbility = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, pullRadius);
    }

}