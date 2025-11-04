using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ItemSpawnerManager;

public class Collector : PlayableCharacter
{
    [Header("Collector")]
    public int maxItems = 4;
    public Transform[] carryPoints;
    public List<GameObject> inventory = new();

    [Header("Interaction Override")]
    public float itemInteractRadius = 1.2f; // smaller value than in PChar class

    [Header("Goo trail")]
    public GameObject gooPrefab;
    public float gooInterval = 1f;

    [Header("Pickup visuals")]
    public float pickedScale = 0.6f;
    public float stackY = 0.3f;


    void Start()
    {
        StartCoroutine(GooTrailRoutine()); // start spawning goo
    }

    protected override void TryInteract()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, itemInteractRadius, itemsLayer);
        Debug.Log($"TryInteract found {hits.Length} hits");
        foreach (var hit in hits)
        {
            Debug.Log($"Hit: {hit.name}, Layer: {LayerMask.LayerToName(hit.gameObject.layer)}");
        }

        if (hits.Length == 0) return;

        foreach (var hit in hits)
        {
            var door = hit.GetComponent<Door>();
            if (door != null)
            {
                door.TryOpen(this);
                return;
            }
        }

        var nearest = GetNearestInteractable(hits, itemInteractRadius);
        if (nearest != null) InteractWith(nearest);

    }

    protected override void InteractWith(Collider2D target)
    {
        if (target == null) return;

        #region Door
        Door door = target.GetComponent<Door>(); // checking if it's a door
        if (door != null)
        {
            door.TryOpen(this);
            return;
        }
        #endregion

        #region Item
        Item item = target.GetComponent<Item>(); // checking if it's an item
        if (item == null) return;

        if (inventory.Count < maxItems)
        {
            PickItem(item); // take if empty slots
            inventory.Add(item.gameObject);
        }
        else
        {
            StartCoroutine(DropUp(target.gameObject)); // drop if full
        }
        #endregion
    }

    public override void SpecialAbility()
    {
        // None
    }

    private void PickItem(Item item)
    {
        if (item == null) return;
        item.OnPicked();
        item.inInventory = true; // cheking so telekinetic can ignore them

        #region Disabling collider and physics for the piccked item so it won't interfere while beign attached to collector

        var col = item.GetComponent<Collider2D>(); // disabling collider
        if (col != null) col.enabled = false;
        var rb = item.GetComponent<Rigidbody2D>(); // disabling rigidbody
        if (rb != null) { rb.velocity = Vector2.zero; rb.isKinematic = true; }
        #endregion

        #region Set Collector as parent (scale & attach)

        Transform targetPoint = null; // check if space to attach is avaible
        for (int i = 0; i < carryPoints.Length; i++)
        {
            bool occupied = inventory.Any(inv =>
                inv.transform.parent == carryPoints[i]);

            if (!occupied)
            {
                targetPoint = carryPoints[i];
                break;
            }
        }

        item.transform.SetParent(targetPoint, worldPositionStays: false);
        item.transform.localPosition = new Vector3(0f, stackY * inventory.Count, 0f); // for item points
        item.transform.localScale = Vector3.one * pickedScale; // scale size
        #endregion

        item.SetVisible(true);
        StartCoroutine(DropUp(item.gameObject));
    }

    private IEnumerator DropUp(GameObject item) // Drop the item if full
    {
        if (item == null) yield break;

        Vector3 start = item.transform.position;
        Vector3 end = start + Vector3.up * 1f;

        float duration = 0.3f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            item.transform.position = Vector3.Lerp(start, end, t / duration);
            yield return null;
        }
    }

    IEnumerator GooTrailRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(gooInterval);
            if (gooPrefab)
                Instantiate(gooPrefab, transform.position, Quaternion.identity);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, itemInteractRadius);

        if (carryPoints == null || carryPoints.Length == 0) return;

        Gizmos.color = Color.yellow;
        foreach (var point in carryPoints)
        {
            if (point != null)
            {
                Gizmos.DrawWireSphere(point.position, 0.15f);
                UnityEditor.Handles.Label(point.position + Vector3.up * 0.2f, point.name);
            }
        }

    }

}