using UnityEngine;

public sealed class Enemy : MonoBehaviour
{
    [SerializeField, Min(0f)] private float dieVelocity = 5f;

    private bool isDefeated;

    private void Start()
    {
        if (LevelManager.HasInstance)
        {
            LevelManager.Instance.RegisterEnemy(this);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDefeated)
        {
            return;
        }

        bool hitByBird = collision.collider.GetComponentInParent<Bird>() != null;
        bool receivedStrongImpact = collision.relativeVelocity.magnitude >= dieVelocity;

        if (!hitByBird && !receivedStrongImpact)
        {
            return;
        }

        Defeat();
    }

    internal void Defeat()
    {
        if (isDefeated)
        {
            return;
        }

        isDefeated = true;
        if (LevelManager.HasInstance)
        {
            LevelManager.Instance.UnregisterEnemy(this);
        }

        Destroy(gameObject);
    }
}
