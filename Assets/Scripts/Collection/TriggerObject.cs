using UnityEngine;

public class TriggerObject : DraggableObject
{
    [SerializeField] protected string triggerTag;
    protected GameObject currentTarget;

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
                else if (currentTarget != null)
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
        if (currentTarget != null)
        {
            SpriteRenderer targetRenderer = currentTarget.GetComponent<SpriteRenderer>();
            if (targetRenderer != null)
            {
                targetRenderer.material = canMergeMaterial;
            }
        }
    }

    protected override bool IsValidPlacement()
    {
        if (!isDragging) return true;

        Collider2D[] colliders = Physics2D.OverlapBoxAll(
            transform.position,
            col.bounds.size,
            transform.rotation.eulerAngles.z
        );

        foreach (Collider2D otherCol in colliders)
        {
            if (otherCol != col && otherCol.gameObject != gameObject)
            {
                var triggerable = otherCol.GetComponent<ITriggerable>();
                if (triggerable != null && triggerable.CanTrigger(gameObject))
                {
                    currentTarget = otherCol.gameObject;
                    triggerable.OnTriggerStart(gameObject);
                    return true;
                }

                var collectible = otherCol.GetComponent<CollectibleObject>();
                if (collectible != null && CompareTag(collectible.GetUnlockMethod().ToString()))
                {
                    currentTarget = otherCol.gameObject;
                    return true;
                }
            }
        }
        
        if (currentTarget != null)
        {
            var triggerable = currentTarget.GetComponent<ITriggerable>();
            if (triggerable != null)
            {
                triggerable.OnTriggerEnd(gameObject);
            }
            currentTarget = null;
        }
        
        return base.IsValidPlacement();
    }

    protected override void CompletePlace()
    {
        if (currentTarget != null && !isInvalidPosition)
        {
            var triggerable = currentTarget.GetComponent<ITriggerable>();
            if (triggerable != null)
            {
                triggerable.OnTriggerComplete(gameObject);
                gameObject.SetActive(false);
            }
            else
            {
                var collectible = currentTarget.GetComponent<CollectibleObject>();
                if (collectible != null)
                {
                    collectible.HandleTriggerEffect();
                    gameObject.SetActive(false);
                }
            }
        }
        base.CompletePlace();
    }
}