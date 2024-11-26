using UnityEngine;

public class DropUnlockChecker : UnlockConditionChecker
{
    [SerializeField] private float requiredImpactForce = 5f;
    private bool hasDropped = false;
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasDropped && collision.relativeVelocity.magnitude > requiredImpactForce)
        {
            hasDropped = true;
        }
    }
    
    public override bool CheckUnlockCondition()
    {
        return hasDropped;
    }
}