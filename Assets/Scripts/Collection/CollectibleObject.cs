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
        if (IsUnlocked()) 
            return base.IsValidPlacement();

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
                TriggerObject trigger = otherCol.GetComponent<TriggerObject>();
                if (trigger != null && !IsUnlocked())
                {
                    // 检查是否匹配的触发组合
                    if (trigger.CompareTag(data.unlockMethod.ToString()))
                    {
                        currentMergeTarget = otherCol.gameObject;
                        return true;
                    }
                }
            }
        }
        
        currentMergeTarget = null;
        return base.IsValidPlacement();
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
        }
        Unlock();
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

    public bool IsUnlocked()
    {
        return PlayerPrefs.GetInt($"Collectible_{data.type}", 0) == 1;
    }
    public UnlockMethod GetUnlockMethod() => data.unlockMethod;

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        // 清理引用和事件监听
        if (data != null)
        {
            data = null;
        }
        
        if (unlockVisualEffect != null)
        {
            Destroy(unlockVisualEffect);
            unlockVisualEffect = null;
        }
        
        currentMergeTarget = null;
        isUnlocked = false;
    }

    // public static void ClearStaticReferences()
    // {
    //     // 清理任何静态引用
    //     allCollectibles.Clear();
    // }
}