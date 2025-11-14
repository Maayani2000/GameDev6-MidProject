using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public class EnemyAI : MonoBehaviour
{
    enum State { Patrol, Chase, Return, Disabled }
    State state = State.Patrol;

    [SerializeField] private Transform visionCone;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Refs")]
    public PatrolMover mover;
    public EnemyViewCone vision;
    public EnemyBase baseComp;

    [Header("Chase")]
    public float chaseSpeedMultiplier = 1.4f;
    public float lostSightDelay = 2.0f; // time to wait after losing the target before returning back
    public float returnTolerance = 0.3f;
    private float lastSeenTime = -999f;

    Vector2 lastPatrolPosition;
    Coroutine lostSightCoroutine;
    Transform currentTarget;

    void Awake()
    {
        baseComp = GetComponent<EnemyBase>();
        if (mover == null) mover = GetComponent<PatrolMover>();
        if (vision == null) vision = GetComponentInChildren<EnemyViewCone>();
        lastPatrolPosition = transform.position;
    }

    void Update()
    {
        if (baseComp == null || baseComp.frozen) return;

        switch (state)
        {
            case State.Patrol:
                HandlePatrol();
                break;
            case State.Chase:
                HandleChase();
                break;
            case State.Return:
                HandleReturn();
                break;
        }

        var seen = vision != null ? vision.GetVisibleTarget() : null; // check vision
        if (seen != null)
        {
            currentTarget = seen; // switch to chasing behaviour
            EnterChase();
        }
    }

    void HandlePatrol()
    {
        lastPatrolPosition = transform.position; // optionally record previous position to return to (before chase)
    }

    //void OnAlarmTriggered()

    #region Chase
    void EnterChase()
    {
        if (state == State.Chase) return;

        lastPatrolPosition = transform.position; // remember where the position before the chase to return there
        state = State.Chase;
        if (mover != null) mover.moveSpeed *= chaseSpeedMultiplier;  // speed up movement
        if (lostSightCoroutine != null) { StopCoroutine(lostSightCoroutine); lostSightCoroutine = null; } // stop any return actions
    }

    void HandleChase()
    {
        if (currentTarget != null)
        {
            if (vision.CanSee(currentTarget))
                lastSeenTime = Time.time;
            else if (Time.time - lastSeenTime > lostSightDelay)
                EnterReturn();
        }

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null && currentTarget != null)
        {
            Vector2 pos = rb.position;
            Vector2 targetPos = currentTarget.position;
            Vector2 dir = (targetPos - pos);
            if (dir.sqrMagnitude > 0.0001f)
            {
                dir = dir.normalized;
                rb.MovePosition(pos + dir * mover.moveSpeed * Time.fixedDeltaTime);

                if (visionCone != null)
                    visionCone.right = dir;

                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = dir.x < 0;
                }
            }
        }

        var seen = vision.GetVisibleTarget(); // check visibility
        if (seen != null)
        {
            lastSeenTime = Time.time;
            currentTarget = seen; // ensure chasing the target
        }
        else
        {
            if (Time.time - lastSeenTime > lostSightDelay)
                EnterReturn();
        }
    }

    #endregion

    #region return

    IEnumerator LostSightCountdown()
    {
        yield return new WaitForSeconds(lostSightDelay);
        lostSightCoroutine = null;

        if (vision.GetVisibleTarget() == null)// double check visibility before returning
            EnterReturn();// go back to patrol asfter the delay

    }

    void StartLostSightTimer()
    {
        if (lostSightCoroutine == null)
            lostSightCoroutine = StartCoroutine(LostSightCountdown());
    }

    void EnterReturn()
    {
        if (state == State.Return) return;
        state = State.Return;
        if (mover != null) mover.moveSpeed /= chaseSpeedMultiplier; // restore speed
        mover.GoToPosition(lastPatrolPosition); // chnage mover target back to last patrol position
    }

    void HandleReturn()
    {
        if (Vector2.Distance(transform.position, lastPatrolPosition) <= returnTolerance) // check if close enough to last patrol position
        {
            state = State.Patrol;
            if (mover != null) mover.SetNextTarget(); // resume path
        }
    }
    #endregion

    // if freezed = stop the movement
}