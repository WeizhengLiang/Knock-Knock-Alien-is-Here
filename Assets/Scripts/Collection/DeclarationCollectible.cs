// 星际友好宣言（摔落解锁）

using UnityEngine;

public class DeclarationCollectible : CollectibleObject
{
    [SerializeField] private float unlockForce = 5f;
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //允许已解锁的宣言继续响应碰撞
        if (data.unlockMethod == UnlockMethod.Drop)
        {
            float impactForce = collision.relativeVelocity.magnitude;
            if (impactForce > unlockForce)
            {
                // 只在未解锁时解锁
                if (!isUnlocked)
                {
                    Unlock();
                }
            }
        }
    }
}