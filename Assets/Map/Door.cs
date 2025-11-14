using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    public int requiredKeyLevel = 1;
    public float fadeSpeed = 0.5f;

    private Collider2D col;
    private SpriteRenderer sr;
    private bool isOpen = false;
    private Light2D doorLight;

    public TextMeshPro DoorStatus;
    private Coroutine messageRoutine;

    // tracking opened doors per keyLevel
    private static Dictionary<int, int> doorsPerLevel = new();
    private static Dictionary<int, int> openedDoorsPerLevel = new();

    void Awake()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        doorLight = GetComponent<Light2D>();

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
            StartCoroutine(OpenDoorAnimation());
            
            int totalDoorsOfThisLevel = FindObjectsOfType<Door>().Count(d => d.requiredKeyLevel == requiredKeyLevel);

            if (!openedDoorsPerLevel.ContainsKey(requiredKeyLevel))
                openedDoorsPerLevel[requiredKeyLevel] = 0;

            openedDoorsPerLevel[requiredKeyLevel]++;

            if (openedDoorsPerLevel[requiredKeyLevel] >= totalDoorsOfThisLevel) // if all doors with the same key level been opened > remove key
            {
                collector.inventory.Remove(keyToRemove);
                Destroy(keyToRemove);
                Debug.Log($"All key lvl {requiredKeyLevel} doors opened. Key removed from inventory.");
                ShowMessage($"All key lvl {requiredKeyLevel} doors opened. Key removed from inventory.", 3f);
            }

            Debug.Log($"Access granted (key lvl {requiredKeyLevel}). Door is now open.");
            ShowMessage($"Access granted (key lvl {requiredKeyLevel}). Door is now open.", 2f);

        }
        else
        {
            Debug.Log($"Access denied. Need key lvl {requiredKeyLevel}.");
            ShowMessage($"Access denied. Need key lvl {requiredKeyLevel}.", 2f);
        }
    }

    private IEnumerator OpenDoorAnimation()
    {
        isOpen = true;
        col.enabled = false;

        if (doorLight != null) // disable Light2D component
            doorLight.enabled = false;

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
            DoorStatus.text = " ";
        }
    }
    #region text update
    private void ShowMessage(string message, float duration = 2f) //text and duratrion to be shown (in sec)
    {
        if (DoorStatus == null) return;

        if (messageRoutine != null)
            StopCoroutine(messageRoutine);

        messageRoutine = StartCoroutine(ShowMessageRoutine(message, duration));
    }

    private IEnumerator ShowMessageRoutine(string message, float duration)
    {
        DoorStatus.text = message;
        yield return new WaitForSeconds(duration);
        DoorStatus.text = string.Empty;
    }
    #endregion
}