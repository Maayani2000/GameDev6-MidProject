using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum Type { None, Keycard, ShipPart } // define if a prefab is a key or a part
    public Type itemType = Type.None;
    public int keyLevel = 1; // index needed to be added for each keycard seperatlu (on keylv 2 = 2)
    public int PartNum = 1;

    [HideInInspector] public bool inInventory = false; // for avoiding unessesary interaction

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        //var winCondition = FindObjectOfType<WinCondition>();
        //winCondition?.RegisterCollectable(this);
    }

    void Start()
    {
        var winCondition = FindObjectOfType<WinCondition>();
        winCondition?.RegisterCollectable(this);
    }

    public bool IsShipPart(int partNum)
    {
        return itemType == Type.ShipPart && PartNum == partNum;
    }

    public void SetVisible(bool visible)
    {
        if (sr != null)
        {
            var c = sr.color;
            c.a = visible ? 1f : 0f;
            sr.color = c;
        }
    }
    public void OnPicked()
    {
        inInventory = true;

        string info = itemType switch
        {
            Type.Keycard => $"lvl {keyLevel}",
            Type.ShipPart => $"part #{PartNum}",
            _ => string.Empty
        };


        Debug.Log($"{itemType} {info} picked and added to inventory");

        //var winCondition = FindObjectOfType<WinCondition>();
        //winCondition?.MarkCollected(this);
    }

    public void OnDropped(Vector3 worldPos) // if we want object to fall back if getting damaged b7ut is optional
    {
        inInventory = false;
        transform.position = worldPos;
        SetVisible(true);
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.isKinematic = false;
        Debug.Log($"{itemType} dropped to world at {worldPos}");
    }
}
