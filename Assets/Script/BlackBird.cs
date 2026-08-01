using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class BlackBird : Bird
{
    [Header("爆発能力")]
    [SerializeField, Min(0.1f)] private float explosionRadius = 3f;
    [SerializeField, Min(0f)] private float explosionForce = 12f;
    [SerializeField, Min(0f)] private float autoExplosionDelay = 1f;
    [SerializeField] private ParticleSystem explosionEffectPrefab;

    [Header("衝突後の表示")]
    [SerializeField] private Color collisionColor = Color.red;

    private SpriteRenderer[] spriteRenderers;
    private bool hasExploded;
    private Coroutine autoExplosionRoutine;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    protected override void ActivateAbility()
    {
        Explode();
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);

        if (!hasExploded && autoExplosionRoutine == null)
        {
            ChangeCollisionColor();
            autoExplosionRoutine = StartCoroutine(ExplodeAfterDelay());
        }
    }

    protected override void OnDestroy()
    {
        if (autoExplosionRoutine != null)
        {
            StopCoroutine(autoExplosionRoutine);
        }

        base.OnDestroy();
    }

    private IEnumerator ExplodeAfterDelay()
    {
        yield return new WaitForSecondsRealtime(autoExplosionDelay);
        autoExplosionRoutine = null;
        Explode();
    }

    private void ChangeCollisionColor()
    {
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.color = collisionColor;
        }
    }

    private void Explode()
    {
        if (hasExploded)
        {
            return;
        }

        hasExploded = true;
        Vector2 explosionPosition = transform.position;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(explosionPosition, explosionRadius);
        var affectedBodies = new HashSet<Rigidbody2D>();
        var defeatedEnemies = new HashSet<Enemy>();

        foreach (Collider2D hitCollider in hitColliders)
        {
            Enemy enemy = hitCollider.GetComponentInParent<Enemy>();
            if (enemy != null && defeatedEnemies.Add(enemy))
            {
                enemy.Defeat();
            }

            Rigidbody2D hitBody = hitCollider.attachedRigidbody;
            if (hitBody == null || hitBody == Body || !affectedBodies.Add(hitBody))
            {
                continue;
            }

            Vector2 direction = hitBody.worldCenterOfMass - explosionPosition;
            Vector2 closestPoint = hitCollider.ClosestPoint(explosionPosition);
            float distance = Vector2.Distance(explosionPosition, closestPoint);
            float forceRate = 1f - Mathf.Clamp01(distance / explosionRadius);

            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                direction = Vector2.up;
            }

            hitBody.WakeUp();
            hitBody.AddForce(direction.normalized * explosionForce * forceRate, ForceMode2D.Impulse);
        }

        PlayExplosionEffect(explosionPosition);
        HideBirdImmediately();
        Destroy(gameObject);
    }

    private void HideBirdImmediately()
    {
        gameObject.SetActive(false);
    }

    private void PlayExplosionEffect(Vector2 position)
    {
        if (explosionEffectPrefab == null)
        {
            return;
        }

        if (explosionEffectPrefab.GetComponentInParent<Bird>() != null)
        {
            Debug.LogError("爆発エフェクトには、鳥とは別のParticleSystemプレハブを設定してください。", this);
            return;
        }

        ParticleSystem effect = Instantiate(explosionEffectPrefab, position, Quaternion.identity);
        effect.Play();

        ParticleSystem.MainModule main = effect.main;
        Destroy(effect.gameObject, main.duration + main.startLifetime.constantMax);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.35f, 0f, 0.35f);
        Gizmos.DrawSphere(transform.position, explosionRadius);
    }
}
