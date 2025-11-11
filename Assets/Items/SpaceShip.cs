using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpaceShip : MonoBehaviour
{
    public int partsInstalled = 0;
    public int totalParts = 4;
    public TextMeshPro progressText; // for inspector or later change to update normal visual UI
    WinCondition winCondition;

    void Awake()
    {
        winCondition = GetComponent<WinCondition>();
        if (winCondition == null)
            winCondition = gameObject.AddComponent<WinCondition>();

        winCondition.spaceShip = this;
    }

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
            winCondition?.TryEvaluate();
        }
    }

    void UpdateUI()
    {
        if (progressText != null)
            progressText.text = $"{partsInstalled}/{totalParts}";
    }
}
