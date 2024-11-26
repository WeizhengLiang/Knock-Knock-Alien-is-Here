// 未命名的囚徒（需要摔碎）

using UnityEngine;

public class SpecimenCollectible : CollectibleObject
{
    [SerializeField] private float breakForce = 8f;
    [SerializeField] private ParticleSystem glassBreakEffect;
    
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isUnlocked && collision.relativeVelocity.magnitude > breakForce)
        {
            if (glassBreakEffect != null)
            {
                glassBreakEffect.Play();
            }
            Unlock();
        }
    }
}