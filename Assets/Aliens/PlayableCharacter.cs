using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class PlayableCharacter : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int maxHp = 100;
    public int currentHp = 100;
    public event Action<int, int> OnHealthChanged;

    [Header("Movement")]
    public float speed = 5f;

    [Header("Interaction")]
    public float interactionRadius = 2.5f;
    public float abilityRadius = 3.5f; // for telekinetic and freezers abilities
    public LayerMask itemsLayer;

    protected Rigidbody2D rb;
    protected Vector2 moveInput;
    protected bool controlsLocked = false;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>() ?? GetComponentInChildren<Rigidbody2D>();
        currentHp = maxHp;
        NotifyHealthChanged();
    }

    protected virtual void Update()
    {
        if (controlsLocked) return;

        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    protected virtual void FixedUpdate()
    {
        if (controlsLocked) return;
        rb.velocity = moveInput * speed;
    }
    protected virtual void TryInteract()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRadius, itemsLayer);
        if (hits.Length > 0)
        {
            var nearest = GetNearestInteractable(hits);
            if (nearest != null) InteractWith(nearest);
        }
    }

    protected Collider2D GetNearestInteractable(Collider2D[] hits, float maxDistance = Mathf.Infinity)
    {
        Collider2D nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var h in hits)
        {
            float d = Vector2.Distance(transform.position, h.transform.position);
            if (d < minDist && d <= maxDistance)
            {
                minDist = d;
                nearest = h;
            }
        }
        return nearest;
    }

    protected abstract void InteractWith(Collider2D target);

    // TODO: Damage Handling

    public virtual void TakeDamage(int amount)
    {
        currentHp -= amount;
        if (currentHp < 0) currentHp = 0;
        NotifyHealthChanged();
        if (currentHp <= 0)
            Die();
    }

    public virtual void Die()
    {
        Debug.Log($"{name} you got caught.");
        gameObject.SetActive(false);
        SceneManager.LoadScene("Lose");
    }

    public void SetControlsLocked(bool locked)
    {
        controlsLocked = locked;
        rb.velocity = Vector2.zero;
    }

    public bool IsControlsLocked() => controlsLocked;

    public abstract void SpecialAbility();

    protected void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(Mathf.Max(0, currentHp), maxHp);
    }
}