using UnityEngine;

public class CollectibleObject : DraggableObject
{
    [Header("收集物设置")]
    [SerializeField] protected CollectibleData data;
    [SerializeField] protected GameObject unlockVisualEffect;
    [SerializeField] protected Animator visualAnimator;
    
    protected bool isUnlocked = false;
    protected Vector2 originalPosition;
    protected GameObject currentMergeTarget;
    
    protected override void Start()
    {
        base.Start();
        originalPosition = transform.position;
    }
    
    protected virtual void UpdateMaterials()
    {
        if (spriteRenderer != null)
        {
            if (isDragging)
            {
                if (isInvalidPosition)
                {
                    spriteRenderer.material = invalidMaterial;
                }
                else if (CheckMergeTarget())
                {
                    spriteRenderer.material = canMergeMaterial;
                    UpdateTargetMaterial();
                }
                else
                {
                    spriteRenderer.material = draggingMaterial;
                }
            }
            else if (isInvalidPosition)
            {
                spriteRenderer.material = invalidMaterial;
            }
            else if (IsMouseOver())
            {
                spriteRenderer.material = canPickupMaterial;
            }
            else
            {
                spriteRenderer.material = normalMaterial;
            }
        }
    }

    protected virtual bool CheckMergeTarget()
    {
        if (!isDragging || isUnlocked) return false;

        Collider2D[] colliders = Physics2D.OverlapBoxAll(
            transform.position,
            col.bounds.size,
            transform.rotation.eulerAngles.z,
            collisionLayer
        );

        foreach (Collider2D otherCol in colliders)
        {
            if (otherCol != col && otherCol.gameObject != gameObject)
            {
                if (IsValidTriggerObject(otherCol.gameObject))
                {
                    currentMergeTarget = otherCol.gameObject;
                    return true;
                }
            }
        }

        currentMergeTarget = null;
        return false;
    }

    protected virtual bool IsValidTriggerObject(GameObject trigger)
    {
        switch (data.unlockMethod)
        {
            case UnlockMethod.PowerSource:
                return trigger.CompareTag("PowerSource");
            case UnlockMethod.Disk:
                return trigger.CompareTag("Disk");
            case UnlockMethod.Rocket:
                return trigger.CompareTag("Rocket");
            default:
                return false;
        }
    }

    private void UpdateTargetMaterial()
    {
        if (currentMergeTarget != null)
        {
            SpriteRenderer targetRenderer = currentMergeTarget.GetComponent<SpriteRenderer>();
            if (targetRenderer != null)
            {
                targetRenderer.material = canMergeMaterial;
            }
        }
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