using UnityEngine;

public sealed class YellowBird : Bird
{
    [Header("加速能力")]
    [SerializeField, Min(1f)] private float speedMultiplier = 2.5f;

    private bool hasActivated;

    protected override void ActivateAbility()
    {
        if (hasActivated || Body.velocity.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        hasActivated = true;
        Body.velocity *= speedMultiplier;
    }
}
