using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerStatusMessageUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PartyManager partyManager;
    [SerializeField] private PlayableCharacter playerOverride;
    [SerializeField] private TMP_Text statusText;

    [Header("Settings")]
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private string hitMessage = "youv'e been hit!";
    [SerializeField] private string otherHitTemplate = "{0} has been hit!";

    private PlayableCharacter trackedCharacter;
    private Coroutine hideRoutine;

    private readonly Dictionary<PlayableCharacter, int> lastHealthValues = new();
    private readonly Dictionary<PlayableCharacter, Action<int, int>> healthSubscriptions = new();
    private readonly List<PlayableCharacter> syncBuffer = new();

    void Awake()
    {
        if (statusText == null)
            statusText = GetComponentInChildren<TMP_Text>(true) ?? GetComponent<TMP_Text>();
    }

    void Start()
    {
        if (partyManager == null)
            partyManager = FindObjectOfType<PartyManager>();

        PlayableCharacter initial = GetLeaderCharacter() ?? playerOverride ?? FindObjectOfType<PlayableCharacter>();
        SetTrackedCharacter(initial);
        SyncPartyMembers();
    }

    void Update()
    {
        var leaderCharacter = GetLeaderCharacter();
        if (leaderCharacter != null && leaderCharacter != trackedCharacter)
            SetTrackedCharacter(leaderCharacter);

        SyncPartyMembers();
    }

    void OnEnable()
    {
        SyncPartyMembers();
    }

    void OnDisable()
    {
        ClearSubscriptions();
    }

    void OnDestroy()
    {
        ClearSubscriptions();
    }

    PlayableCharacter GetLeaderCharacter()
    {
        if (partyManager != null && partyManager.currentLeader != null)
        {
            var leader = partyManager.currentLeader.GetComponent<PlayableCharacter>() ??
                         partyManager.currentLeader.GetComponentInChildren<PlayableCharacter>();
            if (leader != null)
                return leader;
        }
        return null;
    }

    void SetTrackedCharacter(PlayableCharacter character)
    {
        if (trackedCharacter == character)
            return;

        trackedCharacter = character;

        if (trackedCharacter != null)
            lastHealthValues[trackedCharacter] = trackedCharacter.currentHp;

        SyncPartyMembers();
    }

    void SyncPartyMembers()
    {
        syncBuffer.Clear();

        if (partyManager != null && partyManager.members != null)
        {
            foreach (var member in partyManager.members)
            {
                var character = GetPlayableFromPartyMember(member);
                if (character != null && !syncBuffer.Contains(character))
                    syncBuffer.Add(character);
            }
        }

        if (playerOverride != null && !syncBuffer.Contains(playerOverride))
            syncBuffer.Add(playerOverride);

        if (trackedCharacter != null && !syncBuffer.Contains(trackedCharacter))
            syncBuffer.Add(trackedCharacter);

        var existing = new List<PlayableCharacter>(healthSubscriptions.Keys);
        foreach (var character in existing)
        {
            if (!syncBuffer.Contains(character))
                RemoveSubscription(character);
        }

        foreach (var character in syncBuffer)
            EnsureSubscription(character);
    }

    PlayableCharacter GetPlayableFromPartyMember(PartyMember member)
    {
        if (member == null)
            return null;

        return member.GetComponent<PlayableCharacter>() ??
               member.GetComponentInChildren<PlayableCharacter>();
    }

    void EnsureSubscription(PlayableCharacter character)
    {
        if (character == null || healthSubscriptions.ContainsKey(character))
            return;

        Action<int, int> handler = (current, maximum) => HandleHealthChanged(character, current, maximum);
        character.OnHealthChanged += handler;
        healthSubscriptions[character] = handler;
        lastHealthValues[character] = character.currentHp;
    }

    void RemoveSubscription(PlayableCharacter character)
    {
        if (character == null)
            return;

        if (healthSubscriptions.TryGetValue(character, out var handler))
        {
            character.OnHealthChanged -= handler;
            healthSubscriptions.Remove(character);
        }

        lastHealthValues.Remove(character);
    }

    void ClearSubscriptions()
    {
        var existing = new List<PlayableCharacter>(healthSubscriptions.Keys);
        foreach (var character in existing)
            RemoveSubscription(character);
    }

    void HandleHealthChanged(PlayableCharacter character, int current, int maximum)
    {
        if (character == null)
            return;

        if (!lastHealthValues.TryGetValue(character, out var previous))
            previous = current;

        if (current < previous)
        {
            string message = character == trackedCharacter
                ? hitMessage
                : FormatOtherHitMessage(character);

            if (!string.IsNullOrEmpty(message))
                ShowMessage(message);
        }

        lastHealthValues[character] = current;
    }

    string GetCharacterDisplayName(PlayableCharacter character)
    {
        if (character == null)
            return "Party member";

        return character.gameObject != null ? character.gameObject.name : character.name;
    }

    string FormatOtherHitMessage(PlayableCharacter character)
    {
        string displayName = GetCharacterDisplayName(character);
        if (!string.IsNullOrWhiteSpace(otherHitTemplate))
            return string.Format(otherHitTemplate, displayName);

        return $"{displayName} has been hit!";
    }

    void ShowMessage(string message)
    {
        if (statusText == null)
            return;

        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        statusText.text = message;
        hideRoutine = StartCoroutine(HideRoutine());
    }

    IEnumerator HideRoutine()
    {
        yield return new WaitForSeconds(displayDuration);

        if (statusText != null)
            statusText.text = string.Empty;

        hideRoutine = null;
    }
}

