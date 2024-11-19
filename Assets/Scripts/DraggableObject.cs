using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class DraggableObject : MonoBehaviour
{
    [Header("拖拽设置")]
    [SerializeField] protected float dragSpeed = 10f;
    [SerializeField] private LayerMask collisionLayer;

    [Header("基础物体属性")]
    [SerializeField] protected float baseWeight = 1f;
    [SerializeField] protected float baseStrength = 1f;
    
    protected Camera mainCamera;
    protected Rigidbody2D rb;
    protected Collider2D col;
    protected SpriteRenderer spriteRenderer;
    private bool isDragging = false;
    protected bool isInvalidPosition = false;
    protected Color originalColor;
    private static bool isAnyObjectBeingDragged = false;
    private static bool isGlobalFrozen = false;
    private static readonly Vector2 NORMAL_GRAVITY = new Vector2(0, -9.81f);
    private static readonly Vector2 ZERO_GRAVITY = Vector2.zero;

    // 拖拽用的铰链关节
    private HingeJoint2D dragJoint;
    private GameObject dragAnchor;
    
    protected virtual void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        // 初始化时设置碰撞器为非触发器
        col.isTrigger = false;
        // 创建拖拽锚点对象
        CreateDragAnchor();
    }

    private void CreateDragAnchor()
    {
        dragAnchor = new GameObject("DragAnchor_" + gameObject.name);
        Rigidbody2D anchorRb = dragAnchor.AddComponent<Rigidbody2D>();
        anchorRb.bodyType = RigidbodyType2D.Kinematic;
        dragAnchor.SetActive(false);
    }
    
    private void OnMouseDown()
    {
        if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            Debug.Log($"Mouse down on {gameObject.name}. isGlobalFrozen: {isGlobalFrozen}, isInvalidPosition: {isInvalidPosition}");
            
            if (isGlobalFrozen)
            {
                if (isInvalidPosition)
                {
                    Debug.Log($"Starting drag on invalid object: {gameObject.name}");
                    StartDragging();
                }
            }
            else if (!isAnyObjectBeingDragged)
            {
                Debug.Log($"Starting normal drag on: {gameObject.name}");
                StartDragging();
            }
        }
    }
    
    private void StartDragging()
    {
        isDragging = true;
        isAnyObjectBeingDragged = true;
        
        Physics2D.gravity = ZERO_GRAVITY;

        // 设置拖拽点
        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        dragAnchor.transform.position = mousePos;
        dragAnchor.SetActive(true);
        
        // 添加铰链关节
        dragJoint = gameObject.AddComponent<HingeJoint2D>();
        dragJoint.connectedBody = dragAnchor.GetComponent<Rigidbody2D>();
        dragJoint.autoConfigureConnectedAnchor = false;
        dragJoint.connectedAnchor = Vector2.zero;
        
        // 设置本地锚点为鼠标相对物体的位置
        dragJoint.anchor = transform.InverseTransformPoint(mousePos);
        
        col.isTrigger = true;
        rb.isKinematic = false;
        
    }
    
    private void OnMouseUp()
    {
        if (isDragging)
        {
            TryPlaceObject();
        }
    }
    
    private void Update()
    {
        if (isDragging)
        {
            UpdateDragPosition();
            DoUpdatePlacementValidation(); 
        }
    }
    
    // 基础实现
    private void DoUpdatePlacementValidation()
    {
        UpdatePlacementValidation();  // 调用虚方法
    }
    
    private void UpdateDragPosition()
    {
        if (dragAnchor != null)
        {
            Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            dragAnchor.transform.position = mousePos;
        }
    }
    
    protected virtual void UpdatePlacementValidation()
    {
        isInvalidPosition = !IsValidPlacement();
        spriteRenderer.color = isInvalidPosition ? Color.red : originalColor;
    }
    
    private void TryPlaceObject()
    {
        if (!isInvalidPosition)
        {
            // 放置成功
            CompletePlace();
        }
        else
        {
            FreezeInvalidPosition();
        }
    }

    private void FreezeInvalidPosition()
    {
        isDragging = false;
        isAnyObjectBeingDragged = false;
        isGlobalFrozen = true;
        
        // 移除拖拽组件
        if (dragJoint != null)
        {
            Destroy(dragJoint);
        }
        dragAnchor.SetActive(false);

        col.isTrigger = true;
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;
        spriteRenderer.color = Color.red;
        
        // 无效位置时保持零重力
        Physics2D.gravity = ZERO_GRAVITY;
    }
    
    private void CompletePlace()
    {
        isDragging = false;
        isAnyObjectBeingDragged = false;
        isGlobalFrozen = false;
        
        // 移除拖拽组件
        if (dragJoint != null)
        {
            Destroy(dragJoint);
        }
        dragAnchor.SetActive(false);

        // 成功放置时恢复正常碰撞
        col.isTrigger = false;
        rb.isKinematic = false;
        spriteRenderer.color = originalColor;
        
        // 成功放置且没有其他物体被拖动或处于无效位置时恢复重力
        if (!isAnyObjectBeingDragged && !isGlobalFrozen)
        {
            Physics2D.gravity = NORMAL_GRAVITY;
        }
    }
    
    protected virtual bool IsValidPlacement()
    {
        Vector2 position = transform.position;
        float checkRadius = col.bounds.extents.magnitude * 0.9f;
        
        // 检查是否与其他物体重叠
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, checkRadius, collisionLayer);
        foreach (Collider2D otherCol in colliders)
        {
            if (otherCol != col && otherCol.gameObject != gameObject)
            {
                return false;  // 发现重叠就返回 false
            }
        }
        return true;
    }

        // 添加物体特性检查方法
    protected bool CheckPlacementStrength(Collider2D otherCol)
    {
        DraggableObject otherObject = otherCol.GetComponent<DraggableObject>();
        if (otherObject != null)
        {
            // 检查重量限制
            if (transform.position.y > otherCol.transform.position.y)
            {
                if (GetWeight() > otherObject.GetStrength())
                {
                    return false;
                }
            }
            
            // 检查易碎物品
            if (otherObject.IsFragile() && GetWeight() > 1f)
            {
                return false;
            }
        }
        return true;
    }
    
    private void OnEnable()
    {
        // 如果物体在无效位置被启用，自动进入拖拽状态
        if (isInvalidPosition)
        {
            StartDragging();
        }
    }
    private void OnDestroy() {
        // 清理拖拽锚点
        if (dragAnchor != null)
        {
            Destroy(dragAnchor);
        }
    }
    public static bool IsGlobalFrozen()
    {
        return isGlobalFrozen;
    }

    public static void ResetGlobalState()
    {
        isAnyObjectBeingDragged = false;
        isGlobalFrozen = false;
        Physics2D.gravity = NORMAL_GRAVITY;
    }
     
    // 检查是否应该暂停重力
    private static bool ShouldPauseGravity()
    {
        return isAnyObjectBeingDragged || isGlobalFrozen;
    }

    // 虚函数定义
    public virtual float GetWeight() { return baseWeight; }
    public virtual float GetStrength() { return baseStrength; }
    public virtual bool IsFragile() { return false; }
}