using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpaceShip : MonoBehaviour
{
    [Header("Ship Settings")]
    public int totalPartsNeeded = 4; // number of ship parts required
    public float installDelay = 1f; // small delay between installing parts
    public TextMeshPro shipStatus; // UI text

    private HashSet<int> installedParts = new(); // which parts have been installed
    private bool fullyFixed = false;

    private Coroutine messageRoutine;

    public void TryToFix(Collector collector)
    {
        if (fullyFixed || collector == null) return;

        List<GameObject> partsToRemove = new List<GameObject>();
        bool anyNewPartInstalled = false;

        // Go through player's inventory and find ship parts
        foreach (var obj in collector.inventory)
        {
            if (obj == null) continue;

            Item item = obj.GetComponent<Item>();
            if (item != null && item.itemType == Item.Type.ShipPart)
            {
                if (!installedParts.Contains(item.PartNum))
                {
                    installedParts.Add(item.PartNum);
                    partsToRemove.Add(obj);
                    anyNewPartInstalled = true;
                    Debug.Log($"Installed ship part #{item.PartNum}.");
                    ShowMessage($"Installed ship part #{item.PartNum}.", 3f);
                    UpdateShipStatus();
                }
            }
        }

        foreach (var part in partsToRemove) // check and remove used parts from inventory
        {
            collector.inventory.Remove(part);
            Destroy(part);
        }

        if (anyNewPartInstalled)
        {
            StartCoroutine(InstallFeedback());

            // Check if all parts are installed
            if (installedParts.Count >= totalPartsNeeded)
            {
                OnShipFullyFixed();
            }
        }
        else
        {
            Debug.Log("No new ship parts available to install.");
            ShowMessage("No new ship parts available to install.");
        }
    }

    IEnumerator InstallFeedback()
    {
        ShowMessage("Installing ship parts...", 3f);
        yield return new WaitForSeconds(installDelay);
        ShowMessage("Installation complete.", 2f);
        yield return new WaitForSeconds(1f);
        ShowMessage("");
    }

    void UpdateShipStatus()
    {
        int missing = totalPartsNeeded - installedParts.Count;
        if (missing > 0)
        {
            ShowMessage($"Installed {installedParts.Count}/{totalPartsNeeded} ship parts. {missing} remaining.",3f);
        }
        else
        {
            OnShipFullyFixed();
        }
    }
    void OnShipFullyFixed()
    {
        fullyFixed = true;
        Debug.Log("All parts installed! Ship ready for launch!");
        ShowMessage("All parts installed! Ship ready for launch!", 3f);

        SceneManager.LoadScene("Win");
    }

    #region text update
    private void ShowMessage(string message, float duration = 2f)
    {
        if (shipStatus == null) return;

        if (messageRoutine != null)
            StopCoroutine(messageRoutine);

        messageRoutine = StartCoroutine(ShowMessageRoutine(message, duration));
    }

    private IEnumerator ShowMessageRoutine(string message, float duration)
    {
        shipStatus.text = message;
        yield return new WaitForSeconds(duration);
        shipStatus.text = string.Empty;
    }
    #endregion
}
