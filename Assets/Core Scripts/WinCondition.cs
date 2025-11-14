using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinCondition : MonoBehaviour
{
    [Header("Extraction Requirements")]
    [Tooltip("All party members must be within this radius (in world units) of the ship.")]
    public float extractionRadius = 1.5f;

    [Tooltip("Party manager that tracks all playable characters. If left empty it will be auto-discovered at runtime.")]
    public PartyManager partyManager;

    [Tooltip("The ship that everyone must reach. Auto-filled when attached to the SpaceShip object.")]
    public SpaceShip spaceShip;

    [Header("Collectable Tracking")]
    [Tooltip("Automatically discover every Item present in the scene when this component awakens.")]
    public bool autoDiscoverCollectables = true;

    readonly HashSet<Item> remainingCollectables = new HashSet<Item>();
    bool initialized;
    bool winTriggered;

    void Awake()
    {
        InitializeIfNeeded();
    }

    void OnEnable()
    {
        InitializeIfNeeded();
    }

    void Update()
    {
        TryEvaluate();
    }

    void InitializeIfNeeded()
    {
        if (initialized) return;

        if (spaceShip == null)
            spaceShip = GetComponent<SpaceShip>() ?? FindObjectOfType<SpaceShip>();

        if (partyManager == null)
            partyManager = FindObjectOfType<PartyManager>();

        if (autoDiscoverCollectables)
        {
            remainingCollectables.Clear();
            var items = FindObjectsOfType<Item>(includeInactive: true);
            foreach (var item in items)
            {
                if (item == null || item.inInventory) continue;
                remainingCollectables.Add(item);
            }
        }

        initialized = true;
    }

    public void RegisterCollectable(Item item)
    {
        if (item == null) return;

        InitializeIfNeeded();

        if (item.inInventory)
        {
            remainingCollectables.Remove(item);
            TryEvaluate();
            return;
        }

        remainingCollectables.Add(item);
    }

    public void MarkCollected(Item item)
    {
        if (item == null) return;

        InitializeIfNeeded();
        remainingCollectables.Remove(item);
        TryEvaluate();
    }

    public void TryEvaluate()
    {
        if (!initialized) InitializeIfNeeded();
        if (winTriggered) return;
        if (!AllCollectablesCollected()) return;
        if (!AllMembersAtShip()) return;

        winTriggered = true;
        SceneManager.LoadScene("Win");
    }

    bool AllCollectablesCollected()
    {
        remainingCollectables.RemoveWhere(item => item == null || item.inInventory);
        return remainingCollectables.Count == 0;
    }

    bool AllMembersAtShip()
    {
        if (spaceShip == null)
        {
            spaceShip = FindObjectOfType<SpaceShip>();
            if (spaceShip == null) return false;
        }

        if (partyManager == null)
        {
            partyManager = FindObjectOfType<PartyManager>();
            if (partyManager == null) return false;
        }

        if (partyManager.members == null || partyManager.members.Count == 0)
            return false;

        Vector2 shipPosition = spaceShip.transform.position;

        foreach (var member in partyManager.members)
        {
            if (member == null) return false;

            var playable = member.GetComponent<PlayableCharacter>() ?? member.GetComponentInChildren<PlayableCharacter>();
            if (playable == null || !playable.gameObject.activeInHierarchy)
                return false;

            if (Vector2.Distance(playable.transform.position, shipPosition) > extractionRadius)
                return false;
        }

        return true;
    }
}

