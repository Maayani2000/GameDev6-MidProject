using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpaceShip : MonoBehaviour
{
    public int partsInstalled = 0;
    public int totalParts = 4;
    public TextMeshPro progressText; // for inspector or later change to update normal visual UI
    void Start()
    {
        UpdateUI();
    }
    public void InstallPart()
    {
        partsInstalled++;
        UpdateUI();

        if (partsInstalled >= totalParts)
        {
            Debug.Log("Ship fixed!!");
            // trigger win condition
        }
    }

    void UpdateUI()
    {
        progressText.text = $"{partsInstalled}/{totalParts}";
    }
}
