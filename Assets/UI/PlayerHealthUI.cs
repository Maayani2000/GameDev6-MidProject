using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PartyManager partyManager;
    [SerializeField] private PlayableCharacter playerOverride;
    [SerializeField] private TextMeshProUGUI[] healthTexts;
    [SerializeField] private Image[] healthFillImages;

    PlayableCharacter trackedCharacter;

    void Awake()
    {
        if (healthTexts == null || healthTexts.Length == 0)
            healthTexts = GetComponentsInChildren<TextMeshProUGUI>(true);

        if (healthFillImages == null || healthFillImages.Length == 0)
        {
            var images = GetComponentsInChildren<Image>(true);
            var fills = new List<Image>();
            foreach (var image in images)
            {
                if (image != null && image.type == Image.Type.Filled)
                    fills.Add(image);
            }
            healthFillImages = fills.ToArray();
        }
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
        {
            SetTrackedCharacter(leaderCharacter);
        }
    }

    void OnEnable()
    {
        Subscribe();
        if (trackedCharacter != null)
            UpdateDisplay(trackedCharacter.currentHp, trackedCharacter.maxHp);
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

        if (trackedCharacter != null)
            UpdateDisplay(trackedCharacter.currentHp, trackedCharacter.maxHp);
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
        UpdateDisplay(current, maximum);
    }

    void UpdateDisplay(int current, int maximum)
    {
        if (healthTexts != null)
        {
            string label = maximum > 0 ? $"HP {current}/{maximum}" : $"HP {current}";
            for (int i = 0; i < healthTexts.Length; i++)
            {
                if (healthTexts[i] != null)
                    healthTexts[i].text = label;
            }
        }

        if (healthFillImages != null)
        {
            float fill = maximum > 0 ? Mathf.Clamp01(current / (float)maximum) : 0f;
            for (int i = 0; i < healthFillImages.Length; i++)
            {
                if (healthFillImages[i] != null)
                    healthFillImages[i].fillAmount = fill;
            }
        }
    }
}

