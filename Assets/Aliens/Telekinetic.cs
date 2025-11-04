using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class Telekinetic : PlayableCharacter
{
    [Header("Telekinesis")]
    public float pullRadius = 1.5f; // instead of itemInteractRadius as is not relevant to the original collider
    public float focusDuration = 3f; // instead of cooldown the item has limit floating time

    private bool isUsingAbility = false;
    private PartyManager partyManager;

    void Start()
    {
        partyManager = FindObjectOfType<PartyManager>(); // as party manager does the switchintg, refering so it could provide who is the target
    }

    protected override void TryInteract()
    {
        if (controlsLocked) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pullRadius, itemsLayer);
        if (hits.Length == 0) return;

        var nearest = GetNearestInteractable(hits); // chcking if items that can be interacted with avaible
        if (nearest != null)
        {
            InteractWith(nearest);
            Debug.Log($"Telekinetic TryInteract: found {hits.Length} hits");
            foreach (var h in hits) Debug.Log($"  hit: {h.name}");
        }
    }

    protected override void InteractWith(Collider2D target)
    {
        if (target == null) return;

        Item item = target.GetComponent<Item>();
        if (item != null)
        {
            
            if (item.inInventory) { Debug.Log("Item already in inventory"); return; } // ignore items already in inventory

            StartCoroutine(TeleFocus(item.gameObject)); // start focus coroutine on this item
            return;
        }

    }

    public override void SpecialAbility()
    {
        // TeleFocus
    }

    private IEnumerator TeleFocus(GameObject itemGO)
    {
        if (itemGO == null) yield break;
        if (isUsingAbility) yield break;

        var itemComp = itemGO.GetComponent<Item>();
        if (itemComp != null && itemComp.inInventory) yield break;

        isUsingAbility = true;

        if (partyManager != null) partyManager.SetSwitchingBlocked(true); // so they won't switch while using item
        SetControlsLocked(true);

        if (partyManager != null && partyManager.cameraFollow != null) // update camera to item
            partyManager.cameraFollow.SetTarget(itemGO.transform, true);

        //var outline = itemGO.GetComponent<ItemOutline>();
        //if (outline != null) outline.SetOutline(true);

        float elapsed = 0f;
        Vector3 basePos = itemGO.transform.position;

        while (elapsed < focusDuration)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) break; // press ESC to cancel actiohn ( release the item)

            elapsed += Time.deltaTime;

            // Movement input again as the character's is locked and it won't move by it self
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
            Vector3 move = (Vector3)input * Time.deltaTime * 2f; // adjust speed as needed
            basePos += move;

            // Small Floating effect
            float floatAmt = Mathf.Sin(Time.time * 8f) * 0.05f;
            itemGO.transform.position = basePos + Vector3.up * floatAmt;

            yield return null;
        }

        //if (outline != null) outline.SetOutline(false);

        if (partyManager != null && partyManager.cameraFollow != null)
            partyManager.cameraFollow.SetTarget(this.transform, true);

        SetControlsLocked(false);
        if (partyManager != null) partyManager.SetSwitchingBlocked(false);
        isUsingAbility = false;

        itemGO.transform.position = basePos;

    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, pullRadius);
    }

}