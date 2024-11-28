// 星际友好宣言（摔落解锁）

using UnityEngine;

public class DeclarationCollectible : CollectibleObject
{
    [SerializeField] private float unlockForce = 5f;
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isUnlocked && data.unlockMethod == UnlockMethod.Drop)
        {
            float impactForce = collision.relativeVelocity.magnitude;
            if (impactForce > unlockForce)
            {
                Unlock();
            }
        }
    }
}