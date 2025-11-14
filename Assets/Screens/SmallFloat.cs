using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallFloat : MonoBehaviour
{
    [Header("Floating Settings")]
    public float amplitude = 10f; // how far it floats (in pixels)
    public float frequency = 1f; // how fast it floats
    public bool randomOffset = true; // random start

    private RectTransform rectTransform;
    private Vector2 startPos;
    private float offset;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.anchoredPosition;
        offset = randomOffset ? Random.Range(0f, Mathf.PI * 2f) : 0f;
    }

    void Update()
    {
        float y = Mathf.Sin(Time.time * frequency + offset) * amplitude;
        rectTransform.anchoredPosition = startPos + new Vector2(0f, y);
    }
}