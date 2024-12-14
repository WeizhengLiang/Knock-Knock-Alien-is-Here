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

    [Header("Sorting Order")]
    [SerializeField] private int defaultSortingOrder = 0;
    [SerializeField] private int interactiveSortingOrder = 10;  // 交互时的层级

    [Header("Rotation Settings")]
    [SerializeField] protected float rotationSpeed = 2f;        // 降低默认值
    [SerializeField] protected float rotationDamping = 0.8f;    // 增加阻尼
    [SerializeField] protected float rotationThreshold = 0.1f;  // 添加阈值，防止微小旋转

    [Header("Placement Settings")]
    [SerializeField] protected float surfaceIgnoreDistance = 0.3f;    // 忽略表面检测的距离
    [SerializeField] protected bool showPlacementDebug = true;
    #endregion

    #region Component References
    protected Camera mainCamera;                                     // Main camera reference
    protected Rigidbody2D rb;                                       // Rigidbody component
    protected Collider2D col;                                       // Collider component
    protected SpriteRenderer spriteRenderer;                        // Sprite renderer component
    #endregion

    #region Properties
    public float DragSpeed => dragSpeed;
    public float RotationSpeed => rotationSpeed;
    public float RotationDamping => rotationDamping;
    public float RotationThreshold => rotationThreshold;
    public Material NormalMaterial => normalMaterial;
    public Material DraggingMaterial => draggingMaterial;
    public Material InvalidMaterial => invalidMaterial;
    public Material CanMergeMaterial => canMergeMaterial;
    public Material CanPickupMaterial => canPickupMaterial;
    public int DefaultSortingOrder => defaultSortingOrder;
    public int InteractiveSortingOrder => interactiveSortingOrder;
    public bool IsDragging => state.IsDragging;
    public bool IsInvalidPosition => state.IsInvalidPosition;
    public bool IsMouseOver => state.IsMouseOver;
    #endregion

    #region Systems
    public DraggableObjectState state;
    private GameSystemManager systemManager;
    private DragSystem dragSystem;
    private MaterialSystem materialSystem;
    private PhysicsSystem physicsSystem;
    private PlacementSystem placementSystem;

    public DragSystem DragSystem => dragSystem ?? systemManager?.GetSystem<DragSystem>();
    public MaterialSystem MaterialSystem => materialSystem ?? systemManager?.GetSystem<MaterialSystem>();
    public PhysicsSystem PhysicsSystem => physicsSystem ?? systemManager?.GetSystem<PhysicsSystem>();
    public PlacementSystem PlacementSystem => placementSystem ?? systemManager?.GetSystem<PlacementSystem>();
    #endregion

    #region Unity Lifecycle Methods
    /// <summary>
    /// Initialize components and settings
    /// </summary>
    protected virtual void Start()
    {
        CalculateDragSpeed();
    }

    /// <summary>
    /// Add object to tracking list when enabled
    /// </summary>
    protected virtual void OnEnable()
    {
        DraggableObjectManager.Instance?.RegisterObject(this);
    }

    /// <summary>
    /// Remove object from tracking list when disabled
    /// </summary>
    protected virtual void OnDisable()
    {
        DraggableObjectManager.Instance?.UnregisterObject(this);
    }

    /// <summary>
    /// Update drag position and visual feedback
    /// </summary>
    protected virtual void Update()
    {
        systemManager?.Update();
    }

    /// <summary>
    /// Update drag position and visual feedback
    /// </summary>
    protected virtual void FixedUpdate()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        systemManager?.FixedUpdate();
    }

    /// <summary>
    /// Initialize components and settings
    /// </summary>
    protected virtual void Awake()
    {
        InitializeComponents();
        InitializeSystems();
        CalculateDragSpeed();
    }

    /// <summary>
    /// Cleanup when object is destroyed
    /// </summary>
    protected virtual void OnDestroy()
    {
        systemManager?.Cleanup();
        systemManager = null;

        DraggableObjectManager.Instance?.UnregisterObject(this);
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
        }
        else
        {
            dragSpeed = baseDragSpeed;
        }
    }

    /// <summary>
    /// Handle mouse down event
    /// </summary>
    protected virtual void OnMouseDown()
    {
        if (dragSystem != null)
        {
            dragSystem.OnMouseDown(mainCamera.ScreenToWorldPoint(Input.mousePosition));
        }
    }

    /// <summary>
    /// Handle mouse up event
    /// </summary>
    protected virtual void OnMouseUp()
    {
        if (dragSystem != null)
        {
            dragSystem.OnMouseUp();
        }
    }
    #endregion

    #region Cleanup and Utility Methods
    /// <summary>
    /// Get object's base weight
    /// </summary>
    public virtual float GetWeight() => baseWeight;

    /// <summary>
    /// Get object's base strength
    /// </summary>
    public virtual float GetStrength() => baseStrength;

    /// <summary>
    /// Check if object is fragile
    /// </summary>
    public virtual bool IsFragile() => false;
    #endregion

    #region Debug and Development
    /// <summary>
    /// Draw debug gizmos in Scene view
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showPlacementDebug) return;
        placementSystem?.OnDrawGizmos();
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

    /// <summary>
    /// Update visuals of the object
    /// </summary>
    protected virtual void UpdateVisuals()
    {
        if (materialSystem != null)
        {
            materialSystem.UpdateMaterial();
        }
    }
    #endregion

    #region Mouse Interaction
    /// <summary>
    /// Handle mouse enter event
    /// </summary>
    protected virtual void OnMouseEnter()
    {
        if (!state.IsDragging && !state.IsInvalidPosition && !DraggableObjectManager.Instance.IsGlobalFrozen())
        {
            state.SetMouseOver(true);
            UpdateVisuals();
        }
    }

    /// <summary>
    /// Handle mouse exit event
    /// </summary>
    protected virtual void OnMouseExit()
    {
        state.SetMouseOver(false);
        UpdateVisuals();
    }
    #endregion

    protected virtual void InitializeComponents()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (col == null) col = GetComponent<Collider2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected virtual void InitializeSystems()
    {
        systemManager = new GameSystemManager();
        state = new DraggableObjectState(this);
        
        dragSystem = new DragSystem(this, Camera.main);
        materialSystem = new MaterialSystem(this);
        physicsSystem = new PhysicsSystem(this);
        placementSystem = new PlacementSystem(this, collisionLayer, surfaceIgnoreDistance);

        systemManager.AddSystem(dragSystem);
        systemManager.AddSystem(materialSystem);
        systemManager.AddSystem(physicsSystem);
        systemManager.AddSystem(placementSystem);
    }

    #region Virtual Methods
    /// <summary>
    /// Check if current placement is valid
    /// </summary>
    protected virtual bool IsValidPlacement()
    {
        return placementSystem?.ValidatePlacement() ?? true;
    }

    /// <summary>
    /// Complete the placement of the object
    /// </summary>
    protected virtual void CompletePlace()
    {
        if (dragSystem != null)
        {
            dragSystem.EndDrag();
        }
    }

    /// <summary>
    /// Update the materials based on current state
    /// </summary>
    protected virtual void UpdateMaterials()
    {
        if (materialSystem != null)
        {
            materialSystem.UpdateMaterial();
        }
    }
    #endregion
}
