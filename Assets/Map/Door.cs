using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    public int requiredKeyLevel = 1;
    public float fadeSpeed = 0.5f;

    private Collider2D col;
    private SpriteRenderer sr;
    private bool isOpen = false;

    // tracking opened doors per keyLevel
    private static Dictionary<int, int> doorsPerLevel = new();
    private static Dictionary<int, int> openedDoorsPerLevel = new();

    void Awake()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();

        if (!doorsPerLevel.ContainsKey(requiredKeyLevel))
            doorsPerLevel[requiredKeyLevel] = 0;

        doorsPerLevel[requiredKeyLevel]++;
    }

    public void TryOpen(Collector collector)
    {
        if (isOpen || collector == null) return;

        bool hasKey = false;
        GameObject keyToRemove = null;

        foreach (var obj in collector.inventory) // Check if collector has correct key
        {
            if (obj == null) continue;
            Item item = obj.GetComponent<Item>();
            if (item != null && item.itemType == Item.Type.Keycard && item.keyLevel == requiredKeyLevel)
            {
                hasKey = true;
                keyToRemove = obj;
                break;
            }
        }

        if (hasKey)
        {
            Debug.Log($"Access granted (key lvl {requiredKeyLevel}). Door is now open.");
            StartCoroutine(OpenDoorAnimation());

            if (!openedDoorsPerLevel.ContainsKey(requiredKeyLevel))
                openedDoorsPerLevel[requiredKeyLevel] = 0;

            openedDoorsPerLevel[requiredKeyLevel]++;

            if (openedDoorsPerLevel[requiredKeyLevel] >= doorsPerLevel[requiredKeyLevel]) // if all doors with the same key level been opened > remove key
            {
                collector.inventory.Remove(keyToRemove);
                Destroy(keyToRemove);
                Debug.Log($"All key lvl {requiredKeyLevel} doors opened. Key removed from inventory.");
            }
        }
        else
        {
            Debug.Log($"Access denied. Need key lvl {requiredKeyLevel}.");
        }
    }

    private IEnumerator OpenDoorAnimation()
    {
        isOpen = true;
        col.enabled = false;

        if (sr != null)
        {
            Color start = sr.color;
            Color end = new Color(start.r, start.g, start.b, 0f);
            float t = 0f;

            while (t < fadeSpeed)
            {
                t += Time.deltaTime;
                sr.color = Color.Lerp(start, end, t / fadeSpeed);
                yield return null;
            }

            sr.color = end;
        }
    }
}