using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFreezable : MonoBehaviour
{
    private bool frozen = false;
    private Rigidbody2D rb;
    private MonoBehaviour[] aiScripts;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        aiScripts = GetComponents<MonoBehaviour>();
        // disable specific script by name if needed more than movement
    }

    public void Freeze(float seconds)
    {
        if (frozen) return;
        StartCoroutine(FreezeRoutine(seconds));
    }

    private IEnumerator FreezeRoutine(float seconds) // disable AI movement scripts
    {
        frozen = true;

        foreach (var s in aiScripts)
        {
            if (s == this) continue;
            s.enabled = false;
        }
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(seconds);
        foreach (var s in aiScripts)
        {
            if (s == this) continue;
            s.enabled = true;
        }
        frozen = false;
    }
}
