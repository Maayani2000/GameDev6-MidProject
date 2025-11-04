using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigFurnitureFade : MonoBehaviour
{
    [Header("transparency Settings")]
    public float alphaPercentage = 0.5f;
    public float fadeSpeed = 5f;

    private SpriteRenderer sr;
    private int overlapCount = 0;
    private float targetAlpha = 1f;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        if (sr == null) return;

        Color c = sr.color;
        c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * fadeSpeed);
        sr.color = c;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("partyLayer"))
        {
            overlapCount++;
            targetAlpha = alphaPercentage;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("partyLayer"))
        {
            overlapCount = Mathf.Max(0, overlapCount - 1);
            if (overlapCount == 0)
                targetAlpha = 1f;
        }
    }
}
