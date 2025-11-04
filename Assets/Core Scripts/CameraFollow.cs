using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target to Follow")]
    public Transform target; // who is the follow target (current leader)
    public Vector3 offset = new(0f, 0.5f, -10f); // originally was meant for slow camera delay after the target, but currently relevant mostly for the smoothing ( or if baseSmoth time is not 0 at the start)

    [Header("Smoothing Settings")]
    public float baseSmoothTime = 0.0f; // turned to 0 beacuse otherwise they all have weird motion blur thingy
    public float switchSmoothTime = 0.25f; // how fast camera moving from one target to another
    public float blendOutDuration = 0.6f; // giving fade out from switchSmoothTime to baseSmoothTime, so the camera won't do sudden snapping

    private Vector3 velocity;
    private float currentSmoothTime;
    private float blendTimer;
    private bool isBlending = false;

    void LateUpdate()
    {
        if (target == null) return; // if has no target do nothing

        Vector3 desired = target.position + offset; // always calculate the "desired" position (where it's need to be)

        if (isBlending)
        {
            blendTimer -= Time.deltaTime; // timer to start count frames
            float t = 1f - Mathf.Clamp01(blendTimer / blendOutDuration); // or Clamp(value, 0f, 1f) = keep it between 0f to 1f
            currentSmoothTime = Mathf.Lerp(switchSmoothTime, baseSmoothTime, t); // lerp > how far we are through the blending between 0 to 1 and do that smoothly

            if (blendTimer <= 0f)
            {
                isBlending = false;
                currentSmoothTime = baseSmoothTime;
            }
        }

        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, currentSmoothTime); // do ease out
    }

    public void SetTarget(Transform newTarget, bool smoothSwitch) // Update target and fix the smoothing
    {
        target = newTarget;

        if (smoothSwitch) // ease in
        {
            isBlending = true;
            blendTimer = blendOutDuration;
            currentSmoothTime = switchSmoothTime;
        }
        else // ease out
        {
            isBlending = false;
            currentSmoothTime = baseSmoothTime;
            transform.position = newTarget.position + offset;
        }
    }
}