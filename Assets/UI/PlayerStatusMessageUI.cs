using System.Collections;
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

    private PlayableCharacter trackedCharacter;
    private int lastKnownHp = -1;
    private Coroutine hideRoutine;

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
    }

    void Update()
    {
        var leaderCharacter = GetLeaderCharacter();
        if (leaderCharacter != null && leaderCharacter != trackedCharacter)
            SetTrackedCharacter(leaderCharacter);
    }

    void OnEnable()
    {
        Subscribe();
    }

    void OnDisable()
    {
        Unsubscribe();
    }

    void OnDestroy()
    {
        Unsubscribe();
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

        Unsubscribe();
        trackedCharacter = character;
        Subscribe();

        lastKnownHp = trackedCharacter != null ? trackedCharacter.currentHp : -1;
    }

    void Subscribe()
    {
        if (trackedCharacter != null)
            trackedCharacter.OnHealthChanged += HandleHealthChanged;
    }

    void Unsubscribe()
    {
        if (trackedCharacter != null)
            trackedCharacter.OnHealthChanged -= HandleHealthChanged;
    }

    void HandleHealthChanged(int current, int maximum)
    {
        if (lastKnownHp >= 0 && current < lastKnownHp)
            ShowMessage(hitMessage);

        lastKnownHp = current;
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

