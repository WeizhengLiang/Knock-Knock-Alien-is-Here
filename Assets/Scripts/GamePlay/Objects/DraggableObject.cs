using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all draggable objects in the game
/// Handles drag and drop mechanics, physics, and visual feedback
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class DraggableObject : MonoBehaviour
{
    #region Serialized Fields
    [Header("Drag Settings")]
    [SerializeField] protected float dragSpeed = 5f;                  // Current drag speed
    [SerializeField] protected float baseDragSpeed = 5f;             // Base drag speed before mass influence
    [SerializeField] protected float massInfluence = 0.5f;           // How much mass affects drag speed
    [SerializeField] protected float minDragSpeed = 0.5f;            // Minimum possible drag speed
    [SerializeField] protected float maxDragSpeed = 15f;             // Maximum possible drag speed
    [SerializeField] protected LayerMask collisionLayer;             // Layers to check for collisions

    [Header("Base Object Properties")]
    [SerializeField] protected float baseWeight = 1f;                // Base weight of the object
    [SerializeField] protected float baseStrength = 1f;              // Base strength/durability

    [Header("Material Settings")]
    [SerializeField] protected Material normalMaterial;              // Default material
    [SerializeField] protected Material draggingMaterial;            // Material when being dragged
    [SerializeField] protected Material invalidMaterial;             // Material when in invalid position
    [SerializeField] protected Material canMergeMaterial;            // Material when can be merged
    [SerializeField] protected Material canPickupMaterial;           // Material when can be picked up

    [Header("Animation Settings")]
    [SerializeField] protected Animator objectAnimator;              // Object's animator component
    #endregion

    #region Physics Settings
    protected float dragInertiaMultiplier = 0.3f;                    // Inertia force multiplier after drag
    protected Vector2 lastDragPosition;                              // Last recorded drag position
    protected Vector2 dragVelocity;                                  // Current drag velocity
    protected float velocityUpdateInterval = 0.1f;                   // How often to update velocity
    protected float lastVelocityUpdateTime;                          // Time of last velocity update
    #endregion

    #region Component References
    protected Camera mainCamera;                                     // Main camera reference
    protected Rigidbody2D rb;                                       // Rigidbody component
    protected Collider2D col;                                       // Collider component
    protected SpriteRenderer spriteRenderer;                        // Sprite renderer component
    #endregion

    #region State Variables
    protected bool isDragging = false;
    protected bool isInvalidPosition = false;
    protected Color originalColor;
    protected bool isMouseOver = false;
    private static bool isAnyObjectBeingDragged = false;
    private static bool isGlobalFrozen = false;
    private static bool hasInvalidPlacement = false;
    #endregion

    #region Constants
    private static readonly Vector2 NORMAL_GRAVITY = new Vector2(0, -9.81f);
    private static readonly Vector2 ZERO_GRAVITY = Vector2.zero;
    #endregion

    #region Object Tracking
    // Static list to track all draggable objects
    private static List<DraggableObject> allDraggableObjects = new List<DraggableObject>();
    public static List<DraggableObject> AllDraggableObjects => allDraggableObjects;
    #endregion

    #region Frozen State
    // Store state before freezing
    private Vector2 frozenVelocity;
    private float frozenAngularVelocity;
    private bool wasFrozen = false;
    #endregion

    #region Drag Joint
    private HingeJoint2D dragJoint;
    private GameObject dragAnchor;
    #endregion

    #region Unity Lifecycle Methods
    /// <summary>
    /// Initialize components and settings
    /// </summary>
    protected virtual void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        col.isTrigger = false;
        CreateDragAnchor();
        CalculateDragSpeed();
    }

    /// <summary>
    /// Add object to tracking list when enabled
    /// </summary>
    protected virtual void OnEnable()
    {
        allDraggableObjects.Add(this);
        if (isInvalidPosition)
        {
            StartDragging();
        }
    }

    /// <summary>
    /// Remove object from tracking list when disabled
    /// </summary>
    protected virtual void OnDisable()
    {
        allDraggableObjects.Remove(this);
    }

    /// <summary>
    /// Update drag position and visual feedback
    /// </summary>
    protected virtual void Update()
    {
        if (isDragging && dragJoint != null)
        {
            Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            UpdateDragPosition();

            // Calculate drag velocity
            if (Time.time - lastVelocityUpdateTime > velocityUpdateInterval)
            {
                dragVelocity = (mousePos - lastDragPosition) / velocityUpdateInterval;
                lastDragPosition = mousePos;
                lastVelocityUpdateTime = Time.time;
            }

            DoUpdatePlacementValidation();
            rb.angularVelocity = Mathf.Clamp(rb.angularVelocity, -50f, 50f);
        }
        else if (isInvalidPosition && !isDragging)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        else if (spriteRenderer != null && spriteRenderer.material != normalMaterial && !isMouseOver)
        {
            spriteRenderer.material = normalMaterial;
        }
    }
    #endregion

    #region Drag System Methods
    /// <summary>
    /// Calculate drag speed based on object mass
    /// </summary>
    protected virtual void CalculateDragSpeed()
    {
        if (rb != null)
        {
            float massMultiplier = 1f / (1f + (rb.mass * massInfluence));
            dragSpeed = baseDragSpeed * massMultiplier;
            dragSpeed = Mathf.Clamp(dragSpeed, minDragSpeed, maxDragSpeed);
            Debug.Log($"{gameObject.name} mass: {rb.mass}, drag speed: {dragSpeed}");
        }
        else
        {
            dragSpeed = baseDragSpeed;
        }
    }

    /// <summary>
    /// Create anchor point for drag joint
    /// </summary>
    private void CreateDragAnchor()
    {
        dragAnchor = new GameObject("DragAnchor_" + gameObject.name);
        Rigidbody2D anchorRb = dragAnchor.AddComponent<Rigidbody2D>();
        anchorRb.bodyType = RigidbodyType2D.Kinematic;
        dragAnchor.SetActive(false);
    }

    /// <summary>
    /// Handle mouse down event
    /// </summary>
    private void OnMouseDown()
    {
        lastDragPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        lastVelocityUpdateTime = Time.time;
        dragVelocity = Vector2.zero;

        if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            if (isGlobalFrozen)
            {
                if (isInvalidPosition)
                {
                    StartDragging();
                }
            }
            else if (!isAnyObjectBeingDragged)
            {
                StartDragging();
            }
        }
    }

    /// <summary>
    /// Initialize dragging state
    /// </summary>
    private void StartDragging()
    {
        isDragging = true;
        isAnyObjectBeingDragged = true;

        FreezeAllObjects();

        dragAnchor.transform.position = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        dragAnchor.SetActive(true);

        if (dragJoint != null) Destroy(dragJoint);

        rb.isKinematic = false;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        col.isTrigger = true;

        dragJoint = gameObject.AddComponent<HingeJoint2D>();
        dragJoint.connectedBody = dragAnchor.GetComponent<Rigidbody2D>();
        dragJoint.autoConfigureConnectedAnchor = false;
        dragJoint.connectedAnchor = Vector2.zero;
        dragJoint.anchor = transform.InverseTransformPoint(dragAnchor.transform.position);
    }

    /// <summary>
    /// Handle mouse up event
    /// </summary>
    private void OnMouseUp()
    {
        if (isDragging)
        {
            TryPlaceObject();
        }
    }
    #endregion

    #region Object State Management
    /// <summary>
    /// Freeze object's physics state
    /// </summary>
    private void FreezeObject()
    {
        if (this.isInvalidPosition || this.isDragging)
        {
            return;
        }

        if (!wasFrozen)
        {
            frozenVelocity = rb.velocity;
            frozenAngularVelocity = rb.angularVelocity;

            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true;

            wasFrozen = true;
        }
    }

    /// <summary>
    /// Restore object's physics state
    /// </summary>
    private void UnfreezeObject()
    {
        if (wasFrozen)
        {
            rb.isKinematic = false;
            rb.velocity = frozenVelocity;
            rb.angularVelocity = frozenAngularVelocity;

            wasFrozen = false;
        }
    }
    #endregion

    #region Static Methods
    /// <summary>
    /// Freeze all draggable objects in the scene
    /// </summary>
    private static void FreezeAllObjects()
    {
        foreach (var obj in allDraggableObjects)
        {
            obj.FreezeObject();
        }
    }

    /// <summary>
    /// Unfreeze all draggable objects in the scene
    /// </summary>
    private static void UnfreezeAllObjects()
    {
        foreach (var obj in allDraggableObjects)
        {
            obj.UnfreezeObject();
        }
    }

    /// <summary>
    /// Set global frozen state for all objects
    /// </summary>
    public static void SetGlobalFrozen(bool frozen)
    {
        isGlobalFrozen = frozen;

        if (frozen)
        {
            FreezeAllObjects();
        }
        else
        {
            UnfreezeAllObjects();
        }
    }

    /// <summary>
    /// Check if any object has invalid placement
    /// </summary>
    public static bool HasInvalidPlacement()
    {
        return hasInvalidPlacement;
    }

    /// <summary>
    /// Reset invalid placement state
    /// </summary>
    public static void ResetInvalidPlacement()
    {
        hasInvalidPlacement = false;
    }
    #endregion

    #region Placement and Movement
    /// <summary>
    /// Update drag anchor position
    /// </summary>
    private void UpdateDragPosition()
    {
        if (dragAnchor != null)
        {
            Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 currentAnchorPos = dragAnchor.transform.position;

            Vector2 newPosition = Vector2.Lerp(currentAnchorPos, mousePos, dragSpeed * Time.deltaTime);
            dragAnchor.transform.position = newPosition;
        }
    }

    /// <summary>
    /// Check if current placement is valid
    /// </summary>
    protected virtual bool IsValidPlacement()
    {
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
                ColliderDistance2D distance = Physics2D.Distance(col, otherCol);
                if (distance.distance <= 0.01f)
                {
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Try to place the object and handle the result
    /// </summary>
    protected virtual void TryPlaceObject()
    {
        if (!isInvalidPosition)
        {
            CompletePlace();
        }
        else
        {
            FreezeInvalidPosition();
        }
    }

    /// <summary>
    /// Complete object placement
    /// </summary>
    protected virtual void CompletePlace()
    {
        isDragging = false;
        isAnyObjectBeingDragged = false;
        isGlobalFrozen = false;
        isInvalidPosition = false;

        if (dragJoint != null)
        {
            Destroy(dragJoint);
            dragJoint = null;
        }
        dragAnchor.SetActive(false);

        col.isTrigger = false;
        if (spriteRenderer != null)
        {
            spriteRenderer.material = normalMaterial;
        }

        // Apply inertia
        rb.velocity = dragVelocity * dragInertiaMultiplier;
        
        UnfreezeAllObjects();
    }

    /// <summary>
    /// Handle invalid position state
    /// </summary>
    private void FreezeInvalidPosition()
    {
        isDragging = false;
        isAnyObjectBeingDragged = false;
        isGlobalFrozen = true;

        if (dragJoint != null)
        {
            Destroy(dragJoint);
            dragJoint = null;
        }
        dragAnchor.SetActive(false);

        col.isTrigger = true;
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        if (spriteRenderer != null)
        {
            spriteRenderer.material = invalidMaterial;
        }

        if (objectAnimator != null)
        {
            AnimationManager.Instance.PlayFlickering(objectAnimator);
        }
    }
    #endregion

    #region Visual Feedback
    /// <summary>
    /// Update placement validation and visual feedback
    /// </summary>
    protected virtual void UpdatePlacementValidation()
    {
        bool wasInvalid = isInvalidPosition;
        isInvalidPosition = !IsValidPlacement();

        if (wasInvalid != isInvalidPosition)
        {
            hasInvalidPlacement = isInvalidPosition;
        }

        UpdateMaterials();
    }

    /// <summary>
    /// Base implementation for updating materials
    /// </summary>
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
                else
                {
                    spriteRenderer.material = draggingMaterial;
                }
            }
            else if (isInvalidPosition)
            {
                spriteRenderer.material = invalidMaterial;
            }
            else if (isMouseOver)
            {
                spriteRenderer.material = canPickupMaterial;
            }
            else
            {
                spriteRenderer.material = normalMaterial;
            }
        }
    }

    /// <summary>
    /// Wrapper method for placement validation update
    /// </summary>
    private void DoUpdatePlacementValidation()
    {
        UpdatePlacementValidation();
    }
    #endregion

    #region Cleanup and Utility Methods
    /// <summary>
    /// Cleanup when object is destroyed
    /// </summary>
    protected virtual void OnDestroy()
    {
        // Clean up drag anchor
        if (dragAnchor != null)
        {
            Destroy(dragAnchor);
        }

        allDraggableObjects.Remove(this);
    }

    /// <summary>
    /// Check if objects are globally frozen
    /// </summary>
    public static bool IsGlobalFrozen()
    {
        return isGlobalFrozen;
    }

    /// <summary>
    /// Reset all static state variables
    /// </summary>
    public static void ResetGlobalState()
    {
        isAnyObjectBeingDragged = false;
        isGlobalFrozen = false;
        Physics2D.gravity = NORMAL_GRAVITY;
    }

    /// <summary>
    /// Check if gravity should be paused
    /// </summary>
    private static bool ShouldPauseGravity()
    {
        return isAnyObjectBeingDragged || isGlobalFrozen;
    }

    /// <summary>
    /// Get object's base weight
    /// </summary>
    public virtual float GetWeight()
    {
        return baseWeight;
    }

    /// <summary>
    /// Get object's base strength
    /// </summary>
    public virtual float GetStrength()
    {
        return baseStrength;
    }

    /// <summary>
    /// Check if object is fragile
    /// </summary>
    public virtual bool IsFragile()
    {
        return false;
    }

    /// <summary>
    /// Check if object is in invalid position
    /// </summary>
    public bool IsInInvalidPosition()
    {
        return isInvalidPosition;
    }

    /// <summary>
    /// Check if object is being dragged
    /// </summary>
    public bool IsDragging()
    {
        return isDragging;
    }
    #endregion

    #region Debug and Development
    /// <summary>
    /// Draw debug gizmos in Scene view
    /// </summary>
    private void OnDrawGizmos()
    {
        if (col != null && isDragging)
        {
            // Show actual collider shape
            Gizmos.color = isInvalidPosition ? Color.red : Color.yellow;

            if (col is BoxCollider2D boxCol)
            {
                // Draw rotated bounds for BoxCollider2D
                Matrix4x4 rotationMatrix = Matrix4x4.TRS(
                    transform.position,
                    transform.rotation,
                    Vector3.one
                );
                Gizmos.matrix = rotationMatrix;
                Gizmos.DrawWireCube(boxCol.offset, boxCol.size);
            }
            else if (col is CircleCollider2D circleCol)
            {
                // Draw bounds for CircleCollider2D
                Gizmos.DrawWireSphere(
                    (Vector2)transform.position + circleCol.offset,
                    circleCol.radius
                );
            }
        }
    }
    #endregion

    #region Visual Effects
    /// <summary>
    /// Trigger disappear animation
    /// </summary>
    public void TriggerDisappearAnimation()
    {
        if (objectAnimator != null)
        {
            AnimationManager.Instance.PlayFlickering(objectAnimator);
        }

        StartCoroutine(DisableAfterDelay());
    }

    /// <summary>
    /// Coroutine to disable object after animation
    /// </summary>
    private IEnumerator DisableAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
    }
    #endregion

    #region Mouse Interaction
    /// <summary>
    /// Check if mouse is over the object
    /// </summary>
    protected bool IsMouseOver()
    {
        if (!enabled || !gameObject.activeInHierarchy) return false;

        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        return col.OverlapPoint(mousePosition);
    }

    /// <summary>
    /// Handle mouse enter event
    /// </summary>
    protected virtual void OnMouseEnter()
    {
        if (!isDragging && !isInvalidPosition && !isGlobalFrozen)
        {
            isMouseOver = true;
            UpdateMaterials();
        }
    }

    /// <summary>
    /// Handle mouse exit event
    /// </summary>
    protected virtual void OnMouseExit()
    {
        isMouseOver = false;
        UpdateMaterials();
    }
    #endregion

    #region Static Cleanup
    /// <summary>
    /// Clear all static references
    /// </summary>
    public static void ClearStaticReferences()
    {
        isAnyObjectBeingDragged = false;
        isGlobalFrozen = false;
        hasInvalidPlacement = false;
        allDraggableObjects.Clear();
    }
    #endregion
}
