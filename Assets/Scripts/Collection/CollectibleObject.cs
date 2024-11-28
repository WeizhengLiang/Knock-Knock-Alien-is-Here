using UnityEngine;

public class CollectibleObject : DraggableObject
{
    [Header("收集物设置")]
    [SerializeField] protected CollectibleData data;
    [SerializeField] protected GameObject unlockVisualEffect;
    [SerializeField] protected Animator visualAnimator;
    
    protected bool isUnlocked = false;
    protected GameObject currentMergeTarget;

    protected override bool IsValidPlacement()
    {
        if (isUnlocked) return true;

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
                // 检查是否是有效的触发组合
                if (CheckTriggerInteraction(otherCol.gameObject))
                {
                    currentMergeTarget = otherCol.gameObject;
                    return true;
                }

                // 如果不是有效组合，检查普通碰撞
                ColliderDistance2D distance = Physics2D.Distance(col, otherCol);
                if (distance.distance <= 0.01f)
                {
                    return false;
                }
            }
        }
        
        currentMergeTarget = null;
        return true;
    }

    protected virtual bool CheckTriggerInteraction(GameObject other)
    {
        return false; // 基类默认不处理触发
    }

    protected override void CompletePlace()
    {
        if (currentMergeTarget != null && !isInvalidPosition)
        {
            HandleTriggerEffect();
        }
        base.CompletePlace();
    }

    public void HandleTriggerEffect()
    {
        if (currentMergeTarget != null)
        {
            currentMergeTarget.SetActive(false);
            Unlock();
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

    protected override void UpdateMaterials()
    {
        if (spriteRenderer != null)
        {
            if (isDragging)
            {
                if (isInvalidPosition)
                {
                    spriteRenderer.material = invalidMaterial;
                }
                else if (currentMergeTarget != null)
                {
                    spriteRenderer.material = canMergeMaterial;
                    UpdateTargetMaterial();
                }
                else
                {
                    spriteRenderer.material = draggingMaterial;
                }
            }
            else
            {
                base.UpdateMaterials();
            }
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

    public bool IsUnlocked() => isUnlocked;
    public UnlockMethod GetUnlockMethod() => data.unlockMethod;
}