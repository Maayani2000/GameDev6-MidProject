using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Freezer : PlayableCharacter
{
    [Header("Freeze Ability")]
    public float freezeRadius = 3f;
    public float freezeDuration = 5f;
    public float cooldown = 4f;
    public Material freezeOutline;

    private bool isOnCooldown = false;
    readonly List<RendererState> activeEffects = new();

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void InteractWith(Collider2D target) { }

    protected override void TryInteract()
    {
        if (isOnCooldown) return;

        var enemies = FindEnemiesInRange();
        var cameras = FindCamerasInRange();

        if (enemies.Count == 0 && cameras.Count == 0) return;

        StartCoroutine(CooldownRoutine());

        foreach (var enemy in enemies)
            StartCoroutine(FreezeEnemyRoutine(enemy));

        foreach (var camera in cameras)
            StartCoroutine(FreezeCameraRoutine(camera));
    }

    public override void SpecialAbility() { }

    List<EnemyBase> FindEnemiesInRange()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, freezeRadius);
        var results = new List<EnemyBase>();

        foreach (var hit in hits)
        {
            var enemy = hit.GetComponentInParent<EnemyBase>();
            if (enemy == null || enemy.frozen) continue;
            if (!results.Contains(enemy))
                results.Add(enemy);
        }

        return results;
    }

    List<SecurityCameraV2> FindCamerasInRange()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, freezeRadius);
        var results = new List<SecurityCameraV2>();

        foreach (var hit in hits)
        {
            var camera = hit.GetComponentInParent<SecurityCameraV2>();
            if (camera == null) continue;
            if (!results.Contains(camera))
                results.Add(camera);
        }

        return results;
    }

    IEnumerator FreezeEnemyRoutine(EnemyBase enemy)
    {
        ApplyOutline(enemy.gameObject);

        float effectDuration = freezeDuration + 1f;
        enemy.DisableTemporarily(effectDuration);

        yield return new WaitForSeconds(effectDuration);

        RemoveOutline(enemy.gameObject);
    }

    IEnumerator FreezeCameraRoutine(SecurityCameraV2 camera)
    {
        float effectDuration = freezeDuration + 1f;
        camera.DisableTemporarily(effectDuration);
        yield return new WaitForSeconds(effectDuration);
    }

    IEnumerator CooldownRoutine()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldown);
        isOnCooldown = false;
    }

    void ApplyOutline(GameObject target)
    {
        if (freezeOutline == null) return;

        var renderers = target.GetComponentsInChildren<SpriteRenderer>();
        foreach (var renderer in renderers)
        {
            if (renderer == null) continue;
            activeEffects.Add(new RendererState(renderer, renderer.material));
            renderer.material = freezeOutline;
        }
    }

    void RemoveOutline(GameObject target)
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var state = activeEffects[i];
            if (state.Renderer == null) { activeEffects.RemoveAt(i); continue; }
            if (state.Renderer.gameObject == target || state.Renderer.transform.IsChildOf(target.transform))
            {
                state.Renderer.material = state.OriginalMaterial;
                activeEffects.RemoveAt(i);
            }
        }
    }

    void OnDisable()
    {
        // Ensure we clean up any outlines if this component is disabled
        foreach (var state in activeEffects)
        {
            if (state.Renderer != null && state.OriginalMaterial != null)
                state.Renderer.material = state.OriginalMaterial;
        }
        activeEffects.Clear();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, freezeRadius);
    }

    struct RendererState
    {
        public SpriteRenderer Renderer { get; }
        public Material OriginalMaterial { get; }

        public RendererState(SpriteRenderer renderer, Material original)
        {
            Renderer = renderer;
            OriginalMaterial = original;
        }
    }

}
