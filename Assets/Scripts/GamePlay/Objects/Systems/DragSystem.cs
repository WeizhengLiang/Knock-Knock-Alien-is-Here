using UnityEngine;

public class DragSystem : IGameSystem
{
    private readonly DraggableObject owner;
    private readonly Camera mainCamera;
    private readonly Rigidbody2D rb;
    
    private HingeJoint2D dragJoint;
    private GameObject dragAnchor;
    
    private Vector2 lastDragPosition;
    private Vector2 dragStartPosition;
    private Vector2 lastDragDirection;
    private Vector2 dragVelocity;
    
    private float lastVelocityUpdateTime;
    private readonly float velocityUpdateInterval = 0.1f;
    private readonly float dragInertiaMultiplier = 0.3f;

    public DragSystem(DraggableObject owner, Camera mainCamera)
    {
        this.owner = owner;
        this.mainCamera = mainCamera;
        this.rb = owner.GetComponent<Rigidbody2D>();
        
        Initialize();
    }

    public void Initialize()
    {
        CreateDragAnchor();
        DraggableObjectEvents.OnDragStateChanged += HandleDragStateChanged;
    }

    public void Update()
    {
        if (!owner.IsDragging) return;
        UpdateDragState();
    }

    public void FixedUpdate() { }  // 不需要物理更新

    private void CreateDragAnchor()
    {
        dragAnchor = new GameObject($"DragAnchor_{owner.name}");
        var anchorRb = dragAnchor.AddComponent<Rigidbody2D>();
        anchorRb.bodyType = RigidbodyType2D.Kinematic;
        dragAnchor.SetActive(false);
    }

    public void StartDrag(Vector2 position)
    {
        lastDragPosition = position;
        dragStartPosition = position;
        lastDragDirection = Vector2.right;
        lastVelocityUpdateTime = Time.time;
        dragVelocity = Vector2.zero;

        dragAnchor.transform.position = position;
        dragAnchor.SetActive(true);

        if (dragJoint != null)
        {
            Object.Destroy(dragJoint);
        }

        dragJoint = owner.gameObject.AddComponent<HingeJoint2D>();
        dragJoint.connectedBody = dragAnchor.GetComponent<Rigidbody2D>();
        dragJoint.autoConfigureConnectedAnchor = false;
        dragJoint.connectedAnchor = Vector2.zero;
        dragJoint.anchor = owner.transform.InverseTransformPoint(dragAnchor.transform.position);
    }

    public void UpdateDrag(Vector2 mousePosition)
    {
        if (dragAnchor == null) return;

        // 更新位置
        Vector2 currentPos = dragAnchor.transform.position;
        Vector2 newPos = Vector2.Lerp(currentPos, mousePosition, owner.DragSpeed * Time.deltaTime);
        dragAnchor.transform.position = newPos;

        // 更新速度
        if (Time.time - lastVelocityUpdateTime > velocityUpdateInterval)
        {
            dragVelocity = (mousePosition - lastDragPosition) / velocityUpdateInterval;
            lastDragPosition = mousePosition;
            lastVelocityUpdateTime = Time.time;
        }

        // 更新旋转
        UpdateRotation(mousePosition);
    }

    private void UpdateRotation(Vector2 mousePosition)
    {
        Vector2 direction = mousePosition - dragStartPosition;
        if (direction.magnitude > 0.01f)
        {
            float angle = Vector2.SignedAngle(lastDragDirection, direction);
            if (Mathf.Abs(angle) > owner.RotationThreshold)
            {
                angle *= owner.RotationDamping;
                owner.transform.Rotate(0, 0, angle * owner.RotationSpeed * Time.deltaTime);
            }
            lastDragDirection = direction;
        }
        dragStartPosition = mousePosition;
    }

    public void EndDrag()
    {
        if (dragJoint != null)
        {
            Object.Destroy(dragJoint);
            dragJoint = null;
        }

        if (dragAnchor != null)
        {
            dragAnchor.SetActive(false);
        }

        // 应用惯性
        if (rb != null)
        {
            Vector2 finalVelocity = dragVelocity / Time.fixedDeltaTime;
            rb.velocity = finalVelocity * dragInertiaMultiplier;
        }
    }

    public void Cleanup()
    {
        DraggableObjectEvents.OnDragStateChanged -= HandleDragStateChanged;
        if (dragAnchor != null)
        {
            Object.Destroy(dragAnchor);
        }
    }

    public void OnMouseDown(Vector2 mousePosition)
    {
        if (!CanStartDrag()) return;

        lastDragPosition = mousePosition;
        dragStartPosition = mousePosition;
        lastDragDirection = Vector2.right;
        lastVelocityUpdateTime = Time.time;
        dragVelocity = Vector2.zero;

        owner.state.SetDragging(true);
        StartDrag(mousePosition);
        
        SoundManager.Instance.PlaySoundFromResources("Sound/Pickup", "Pickup", false, 0.25f);
    }

    public void OnMouseUp()
    {
        if (!owner.IsDragging) return;

        owner.state.SetDragging(false);
        EndDrag();
    }

    private bool CanStartDrag()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return false;
        if (DraggableObjectManager.Instance.IsGlobalFrozen())
        {
            return owner.IsInvalidPosition;
        }
        return !DraggableObjectManager.Instance.IsAnyObjectDragging();
    }

    public void UpdateDragState()
    {
        if (!owner.IsDragging) return;

        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        UpdateDrag(mousePos);

        // 更新速度
        if (Time.time - lastVelocityUpdateTime > velocityUpdateInterval)
        {
            dragVelocity = (mousePos - lastDragPosition) / velocityUpdateInterval;
            lastDragPosition = mousePos;
            lastVelocityUpdateTime = Time.time;
        }

        // 限制角速度
        if (rb != null)
        {
            rb.angularVelocity = Mathf.Clamp(rb.angularVelocity, -50f, 50f);
        }
    }

    private void HandleDragStateChanged(DraggableObject obj, bool isDragging)
    {
        if (obj != owner) return;
        
        if (isDragging)
        {
            PrepareDrag();
        }
        else
        {
            EndDrag();
        }
    }

    public void PrepareDrag()
    {
        if (owner.PhysicsSystem != null)
        {
            owner.PhysicsSystem.PrepareDrag();
        }
    }
} 