using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    [Header("Members")]
    public List<PartyMember> members = new List<PartyMember>(); // create list in inspector

    [Header("Trail")] // setting for how followers "trail" (go after) after leader
    public int maxTrailPoints = 200; // max position to remember (as it goes older pints will get cleraed)
    public float recordInterval = 0.12f; // how often is secsond (lower = more often)
    public float minMoveToRecord = 0.05f; // what is the min distance needed between leader and follor to start remebering the trail
    public int trailSpacingPerFollower = 4; // what is the diatance (by the recorder position points) followers skip to stay spaced (higher = futher from each other)

    [Header("Swicth & Sort")] // how fast they switch (logic not camera) and what is the starting sorting layer
    public float switchDuration = 0.35f;
    public int baseSortingOrder = 0;

    [Header("Layers")] // different ovverides for colliders and preventing/ blocking movement
    public LayerMask partyLayer; // part members
    public LayerMask wallsANDfurnitureLayer; // walls & furniture
    public LayerMask bigObjectsLayer; // big objest so they can go behind it
    public LayerMask enemiesLayer; // enemies (for freezer)
    public LayerMask itemsLayer; // keys ,parts and doors (for collector and telecinetic)
    public LayerMask Default;

    [Header("Main Camera")] // for camera attachment (for updating the target later)
    public CameraFollow cameraFollow;

    // state
    public PartyMember currentLeader { get; private set; }
    public bool isSwitching { get; private set; }

    List<Vector2> trail = new List<Vector2>(); // index/element 0 = newest
    float recordTimer;
    Vector2 lastRecordedPos;

    void Start()
    {
        members.RemoveAll(m => m == null); // clear the list
        if (members.Count == 0) return; // if no members do nothing

        RandomOrder(); // Randomize leader and the list order

        currentLeader = members[0]; // set element/index 0 as leader
        lastRecordedPos = currentLeader.transform.position; // clear past positions
        trail.Clear(); // clear the trail
        trail.Add(lastRecordedPos); // start a new one

        ApplyMemberDefaults();
        UpdateSorting();
        
        cameraFollow.SetTarget(currentLeader.transform, false); // set the leader as target to follow, but turn off smoothing (camera delay)
    }

    void Update()
    {
        if (members == null || members.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.Space) && !isSwitching && members.Count > 1) // when pressing Space go to next in the list
            StartCoroutine(Switch()); // and then do ther switching seq

        RememberTrail(); // remeber postions
    }

    void LateUpdate()
    {
        UpdateSorting();
    }
    void RandomOrder() // Random Leader and list order
    {
        for (int i = 0; i < members.Count; i++)
        {
            int r = UnityEngine.Random.Range(i, members.Count);
            var temp = members[i];
            members[i] = members[r];
            members[r] = temp;
        }
    }
    void RememberTrail()
    {
        if (currentLeader == null) return;

        recordTimer += Time.deltaTime;
        Vector2 p = currentLeader.transform.position;
        if (recordTimer >= recordInterval && Vector2.Distance(p, lastRecordedPos) >= minMoveToRecord)
        {
            recordTimer = 0f;
            lastRecordedPos = p;
            trail.Insert(0, p);
            if (trail.Count > maxTrailPoints) trail.RemoveAt(trail.Count - 1);
        }
    }

    IEnumerator Switch()
    {
        if (isSwitching) yield break;
        isSwitching = true;

        members.RemoveAll(m => m == null);
        if (members.Count < 2) { isSwitching = false; yield break; }

        var old = members[0];
        members.RemoveAt(0);
        members.Add(old);

        currentLeader = members[0];
        ApplyMemberDefaults(); // set locked/unlocked and indices

        cameraFollow.SetTarget(currentLeader.transform, true);

        float t = 0f;
        while (t < switchDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        UpdateSorting();
        isSwitching = false;
    }
    public void SetSwitchingBlocked(bool blocked) // for temorrary limit
    {
        isSwitching = blocked;
    }

    void ApplyMemberDefaults() // unlcok leaders controls and set followers as locked
    {
        for (int i = 0; i < members.Count; i++)
        {
            var pm = members[i];
            if (pm == null) continue;

            pm.SetManager(this);
            pm.SetIndex(i); // leader = 0 |followers = 1
            pm.BecomeLeader(i == 0 ? true : false);

            var pc = pm.GetComponent<PlayableCharacter>() ?? pm.GetComponentInChildren<PlayableCharacter>();
            if (pc != null)
                pc.SetControlsLocked(i != 0);

            pm.obstacleMask = wallsANDfurnitureLayer; // prevent members colliders to bump in to walls / furniture
        }
    }

    public Vector2 GetTrailPointFor(int followerIndex, PartyMember requester) // Party member checks for target point (leader position)
    {
        if (trail.Count == 0) return requester.transform.position;

        int idx = Mathf.Clamp(followerIndex * trailSpacingPerFollower, 0, trail.Count - 1);

        int maxSkip = Mathf.Min(trail.Count - 1, trailSpacingPerFollower * 8); // skip some staps
        for (int s = 0; s <= maxSkip; s += trailSpacingPerFollower)
        {
            int check = Mathf.Clamp(idx + s, 0, trail.Count - 1);
            Vector2 cand = trail[check];
            if (!requester.IsPointBlocked(cand)) return cand;
        }

        return trail[idx];
    }

    void UpdateSorting() // in front / behind
    {
        members.RemoveAll(m => m == null);
        if (members.Count == 0) return;

        var sorted = members.OrderBy(m => m.transform.position.y).ToList();
        for (int i = 0; i < sorted.Count; i++)
            sorted[i].ApplySorting(baseSortingOrder + i);
    }

}