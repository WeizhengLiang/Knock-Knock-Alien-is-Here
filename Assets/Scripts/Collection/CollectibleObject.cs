using UnityEngine;

public class CollectibleObject : DraggableObject
{
    [Header("收集物设置")]
    [SerializeField] protected CollectibleData data;
    [SerializeField] protected GameObject unlockVisualEffect;
    [SerializeField] protected Animator visualAnimator;
    
    protected bool isUnlocked = false;
    protected Vector2 originalPosition;
    
    protected override void Start()
    {
        base.Start();
        originalPosition = transform.position;
    }
    
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isUnlocked && data.unlockMethod == UnlockMethod.Drop)
        {
            float impactForce = collision.relativeVelocity.magnitude;
            if (impactForce > 5f) // 可调整的阈值
            {
                Unlock();
            }
        }
    }
    
    protected virtual void Unlock()
    {
        isUnlocked = true;
        if (unlockVisualEffect != null)
        {
            unlockVisualEffect.SetActive(true);
        }
        CollectibleManager.Instance.UnlockCollectible(data.type);
    }
}