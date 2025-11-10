using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Collider2D))]
public class SecurityCameraV2 : MonoBehaviour
{
    [Header("Pivot + Visuals")]
    public Transform pivot; // rotating
    public Light2D viewConeLight;
    public bool enableLightSync = true;

    [Header("Vision / detection")]
    public EnemyViewCone vision;
    public float viewDistance = 6f; // gizmos
    public float arcAngle = 60f;

    [Header("Rotation")]
    public float sweepSpeed = 20f;// time takes for sweep
    public float sweepArc = 60f; // full sweep arc (degrees)
    public bool sweepEnabled = true;

    [Header("Alert")]
    public float alertHoldTime = 2f; //how long target has to remain seen to trigger alarm
    public float lostSightDelay = 2f;// how long to wait after last seen before returning to regular rotation
    public LayerMask enemiesNotifyMask;
    public float notifyRadius = 8f;

    Transform currentTarget; // Transform location for target
    private float lastSeenTime = -999f; // last seen time
    private float lockTimer = 0f; // time spent following locking target
    private bool isAlerting = false;
    private bool isDisabled = false;
    bool storedSweepEnabled;
    bool storedLightSync;
    bool storedVisionEnabled;
    Coroutine disableRoutine;

    private void OnValidate() // for debuggin
    {
        if (vision != null)
        {
            arcAngle = vision.arcAngle;
            viewDistance = vision.viewDistance;
        }
    }

    void Awake()
    {
        if (vision == null) vision = GetComponentInChildren<EnemyViewCone>();
        if (pivot == null) pivot = transform.Find("Pivot") ?? transform;
        UpdateLightSync();
    }

    void Update()
    {
        if (isDisabled) return;

        UpdateLightSync();
        Transform seen = null;
        if (vision != null)
        {
            var got = vision.GetVisibleTarget();
            if (got != null) seen = got;
        }

        if (seen != null) // update seen and ensure target
        {
            if (currentTarget == null || currentTarget != seen)
            {
                currentTarget = seen;
                lockTimer = 0f; // reset hold timer for alarm
            }
            lastSeenTime = Time.time;
        }

        if (currentTarget != null && Time.time - lastSeenTime <= lostSightDelay) // if have target,track it
        {
            TrackTarget();

            if (seen != null && seen == currentTarget)
            {
                lockTimer += Time.deltaTime;
                if (!isAlerting && lockTimer >= alertHoldTime)
                {
                    TriggerAlarm(pivot != null ? pivot.position : transform.position);
                }
            }
            return;
        }

        if (currentTarget != null && Time.time - lastSeenTime > lostSightDelay)
        {
            currentTarget = null;
            lockTimer = 0f;
            isAlerting = false;
            SetAlertLight(false);
        }

        if (sweepEnabled)
            SweepPivot();
    }

    public void DisableTemporarily(float seconds)
    {
        if (!isDisabled)
        {
            storedSweepEnabled = sweepEnabled;
            storedLightSync = enableLightSync;
            storedVisionEnabled = vision != null && vision.enabled;
            sweepEnabled = false;
            enableLightSync = false;
            if (vision != null) vision.enabled = false;
            SetAlertLight(false);
            isAlerting = false;
            currentTarget = null;
            lockTimer = 0f;
            isDisabled = true;
        }
        else if (disableRoutine != null)
        {
            StopCoroutine(disableRoutine);
        }

        disableRoutine = StartCoroutine(DisableRoutine(seconds));
    }

    IEnumerator DisableRoutine(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        sweepEnabled = storedSweepEnabled;
        enableLightSync = storedLightSync;
        if (vision != null) vision.enabled = storedVisionEnabled;
        isAlerting = false;
        currentTarget = null;
        lockTimer = 0f;
        isDisabled = false;
        disableRoutine = null;
    }

    void TrackTarget()
    {
        if (currentTarget == null || pivot == null) return;

        Vector3 dir = currentTarget.position - pivot.position;
        float angleToTarget = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion targetRot = Quaternion.Euler(0f, 0f, angleToTarget - 90f);

        pivot.rotation = Quaternion.RotateTowards(pivot.rotation, targetRot, sweepSpeed * Time.deltaTime);
    }

    void SweepPivot()
    {
        if (pivot == null) return;

        float half = sweepArc * 0.5f; // follow target angle based on time
        float targetAngle = Mathf.Sin(Time.time * sweepSpeed * 0.01f) * half;

        Quaternion targetRot = Quaternion.Euler(0f, 0f, targetAngle); // rotation from target diraction

        pivot.localRotation = Quaternion.RotateTowards(pivot.localRotation, targetRot, sweepSpeed * Time.deltaTime); // smoothly rotate from current to target
    }

    void UpdateLightSync()
    {
        if (!enableLightSync || viewConeLight == null) return;

        if (pivot != null)
        {
            viewConeLight.transform.position = pivot.position;
            viewConeLight.transform.rotation = pivot.rotation;
        }
        else
        {
            viewConeLight.transform.position = transform.position;
            viewConeLight.transform.rotation = transform.rotation;
        }

        viewConeLight.pointLightInnerAngle = arcAngle;
        viewConeLight.pointLightOuterAngle = arcAngle;
        viewConeLight.pointLightOuterRadius = viewDistance;
    }

    void SetAlertLight(bool alert)
    {
        if (viewConeLight == null) return;
        viewConeLight.color = alert ? Color.red : Color.white;
        viewConeLight.intensity = alert ? 1.5f : 1f;
    }

    void TriggerAlarm(Vector3 origin)
    {
        if (isAlerting) return;
        isAlerting = true;
        Debug.Log($"SecurityCamera: Alarm triggered at {origin} by {currentTarget?.name}");

        SetAlertLight(true);

        // notify nearby enemies (example: push an event or overlap)
        Collider2D[] found = Physics2D.OverlapCircleAll(origin, notifyRadius, enemiesNotifyMask);
        foreach (var c in found)
        {
            if (c == null) continue;
            // Try to find an enemy component (example)
            var enemyAI = c.GetComponentInParent<EnemyAI>();
            if (enemyAI != null)
            {
                // Implement your interfacing method on enemies (example method name)
                // enemyAI.OnAlarmTriggered(origin);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (pivot == null) pivot = transform;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(pivot.position, viewDistance);

        Gizmos.color = Color.cyan;
        Vector3 f = pivot.up;
        Gizmos.DrawLine(pivot.position, pivot.position + f * viewDistance);

        Gizmos.color = Color.green;
        float half = arcAngle * 0.5f;
        Vector3 left = Quaternion.Euler(0, 0, -half) * f;
        Vector3 right = Quaternion.Euler(0, 0, half) * f;
        Gizmos.DrawLine(pivot.position, pivot.position + left * viewDistance);
        Gizmos.DrawLine(pivot.position, pivot.position + right * viewDistance);
    }
}
