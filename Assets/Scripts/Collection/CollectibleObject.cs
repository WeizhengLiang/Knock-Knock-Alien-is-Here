using UnityEngine;

/// <summary>
/// Represents a collectible object that can be unlocked through specific interactions
/// Inherits from DraggableObject to handle drag and drop functionality
/// </summary>
public class CollectibleObject : DraggableObject
{
    #region Serialized Fields
    [Header("Collectible Settings")]
    [SerializeField] protected CollectibleData data;              // Data container for collectible properties
    [SerializeField] protected GameObject unlockVisualEffect;     // Visual effect shown when unlocked
    [SerializeField] protected Animator visualAnimator;           // Animator for visual feedback
    #endregion

    #region Protected Fields
    protected bool isUnlocked = false;                           // Current unlock state
    protected GameObject currentMergeTarget;                      // Current object this can merge with
    #endregion

    #region Placement Validation
    /// <summary>
    /// Checks if the current placement position is valid
    /// Handles special case for unlocked state and trigger interactions
    /// </summary>
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
                    // Check if trigger matches unlock method
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

    /// <summary>
    /// Base implementation for trigger interaction check
    /// </summary>
    protected virtual bool CheckTriggerInteraction(GameObject other)
    {
        return false; // Base class doesn't handle triggers
    }
    #endregion

    #region Placement and Unlock Logic
    /// <summary>
    /// Handles completion of object placement
    /// Triggers unlock effect if conditions are met
    /// </summary>
    protected override void CompletePlace()
    {
        if (currentMergeTarget != null && !isInvalidPosition)
        {
            HandleTriggerEffect();
        }

        base.CompletePlace();
    }

    /// <summary>
    /// Handles the effect when successfully triggered
    /// Disables trigger object and unlocks collectible
    /// </summary>
    public void HandleTriggerEffect()
    {
        if (currentMergeTarget != null)
        {
            currentMergeTarget.SetActive(false);
        }

        Unlock();
    }

    /// <summary>
    /// Unlocks the collectible and triggers visual feedback
    /// </summary>
    protected virtual void Unlock()
    {
        isUnlocked = true;
        if (unlockVisualEffect != null)
        {
            unlockVisualEffect.SetActive(true);
        }

        CollectibleManager.Instance.UnlockCollectible(data.type);
    }
    #endregion

    #region Visual Feedback
    /// <summary>
    /// Updates the material based on current state
    /// Handles special case for merge state
    /// </summary>
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

    /// <summary>
    /// Updates the material of the current merge target
    /// </summary>
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
    #endregion

    #region Public Methods
    /// <summary>
    /// Checks if the collectible is unlocked
    /// </summary>
    public bool IsUnlocked()
    {
        return PlayerPrefs.GetInt($"Collectible_{data.type}", 0) == 1;
    }

    /// <summary>
    /// Gets the unlock method for this collectible
    /// </summary>
    public UnlockMethod GetUnlockMethod() => data.unlockMethod;
    #endregion

    #region Cleanup
    /// <summary>
    /// Cleanup when object is destroyed
    /// </summary>
    protected override void OnDestroy()
    {
        base.OnDestroy();

        // Clean up references and event listeners
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
    #endregion
}