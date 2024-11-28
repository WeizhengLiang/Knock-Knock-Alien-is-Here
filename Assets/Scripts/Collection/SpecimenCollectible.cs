// 未命名的囚徒（需要摔碎）

using UnityEngine;

public class SpecimenCollectible : CollectibleObject
{
    [SerializeField] public float breakForce = 8f;
    [SerializeField] private ParticleSystem glassBreakEffect;
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isUnlocked && data.unlockMethod == UnlockMethod.Break)
        {
            float impactForce = collision.relativeVelocity.magnitude;
            if (impactForce > breakForce)
            {
                if (glassBreakEffect != null)
                {
                    glassBreakEffect.Play();
                }
                Unlock();
            }
        }
    }
}