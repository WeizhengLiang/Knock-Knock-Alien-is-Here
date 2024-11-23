using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class DraggableObject : MonoBehaviour
{
    [Header("拖拽设置")]
    [SerializeField] protected float dragSpeed = 5f;
    [SerializeField] protected float baseDragSpeed = 5f;    // 基础拖拽速度
    [SerializeField] protected float massInfluence = 0.5f;   // 质量对拖拽速度的影响程度
    [SerializeField] protected float minDragSpeed = 0.5f;      // 最小拖拽速度
    [SerializeField] protected float maxDragSpeed = 15f;     // 最大拖拽速度
    [SerializeField] private LayerMask collisionLayer;

    [Header("基础物体属性")]
    [SerializeField] protected float baseWeight = 1f;
    [SerializeField] protected float baseStrength = 1f;

    [Header("材质设置")]
    [SerializeField] protected Material normalMaterial;      // 正常状态材质
    [SerializeField] protected Material draggingMaterial;    // 拖拽状态材质
    [SerializeField] protected Material invalidMaterial;     // 无效位置状态材质

    [Header("动画设置")]
    [SerializeField] protected Animator disappearAnimator;
    
    protected Camera mainCamera;
    protected Rigidbody2D rb;
    protected Collider2D col;
    protected SpriteRenderer spriteRenderer;
    protected bool isDragging = false;
    public bool IsDragging() => isDragging;
    protected bool isInvalidPosition = false;
    public bool IsInInvalidPosition() => isInvalidPosition;
    protected Color originalColor;
    private static bool isAnyObjectBeingDragged = false;
    private static bool isGlobalFrozen = false;
    private static bool hasInvalidPlacement = false;
    private static readonly Vector2 NORMAL_GRAVITY = new Vector2(0, -9.81f);
    private static readonly Vector2 ZERO_GRAVITY = Vector2.zero;

    // 静态列表跟踪所有可拖动物体
    private static List<DraggableObject> allDraggableObjects = new List<DraggableObject>();
    public static List<DraggableObject> AllDraggableObjects => allDraggableObjects;
    
    // 记录冻结前的状态
    private Vector2 frozenVelocity;
    private float frozenAngularVelocity;
    private bool wasFrozen = false;

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
        // 根据质量计算拖拽速度
        CalculateDragSpeed();
    }

    protected virtual void OnEnable()
    {
        allDraggableObjects.Add(this);
        // 如果物体在无效位置被启用，自动进入拖拽状态
        if (isInvalidPosition)
        {
            StartDragging();
        }
    }
    
    protected virtual void OnDisable()
    {
        allDraggableObjects.Remove(this);
    }

    private static void FreezeAllObjects()
    {
        foreach (var obj in allDraggableObjects)
        {
            obj.FreezeObject();
        }
    }
    
    private static void UnfreezeAllObjects()
    {
        foreach (var obj in allDraggableObjects)
        {
            obj.UnfreezeObject();
        }
    }
    
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
    
    private void UnfreezeObject()
    {
        if (wasFrozen)
        {
            // 恢复物体状态
            rb.isKinematic = false;
            rb.velocity = frozenVelocity;
            rb.angularVelocity = frozenAngularVelocity;
            
            wasFrozen = false;
        }
    }
    
    // 添加静态方法来控制全局冻结状态
    public static void SetGlobalFrozen(bool frozen)
    {
        isGlobalFrozen = frozen;
        
        // 如果冻结，停止所有物体的拖拽
        if (frozen)
        {
            FreezeAllObjects();
        }
        else
        {
            UnfreezeAllObjects();
        }
    }

    protected virtual void CalculateDragSpeed()
    {
        if (rb != null)
        {
            // 质量越大，速度越慢
            float massMultiplier = 1f / (1f + (rb.mass * massInfluence));
            dragSpeed = baseDragSpeed * massMultiplier;
            
            // 限制在最小和最大速度范围内
            dragSpeed = Mathf.Clamp(dragSpeed, minDragSpeed, maxDragSpeed);
            
            // 输出调试信息
            Debug.Log($"{gameObject.name} 的质量: {rb.mass}, 拖拽速度: {dragSpeed}");
        }
        else
        {
            dragSpeed = baseDragSpeed;
        }
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
            if (isGlobalFrozen)
            {
                if (isInvalidPosition)
                {
                    // 即使是运动学状态也可以开始拖动
                    StartDragging();
                }
            }
            else if (!isAnyObjectBeingDragged)
            {
                StartDragging();
            }
        }
    }
    
    private void StartDragging()
    {
        isDragging = true;
        isAnyObjectBeingDragged = true;
        
        // 冻结其他物体
        FreezeAllObjects();
        
        // 设置拖拽物体的状态
        dragAnchor.transform.position = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        dragAnchor.SetActive(true);
        
        if (dragJoint != null) Destroy(dragJoint);
        
        // 确保可以拖动
        rb.isKinematic = false;  // 临时设为非运动学以便拖动
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        col.isTrigger = true;
        
        // 设置铰链关节
        dragJoint = gameObject.AddComponent<HingeJoint2D>();
        dragJoint.connectedBody = dragAnchor.GetComponent<Rigidbody2D>();
        dragJoint.autoConfigureConnectedAnchor = false;
        dragJoint.connectedAnchor = Vector2.zero;
        dragJoint.anchor = transform.InverseTransformPoint(dragAnchor.transform.position);

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
        if (isDragging && dragJoint != null)
        {
            Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            UpdateDragPosition();
            DoUpdatePlacementValidation();
        }
        else if (isInvalidPosition && !isDragging)
        {
            // 确保无效位置的物体保持静止
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
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
            Vector2 currentAnchorPos = dragAnchor.transform.position;
            
            // 使用 dragSpeed 来插值移动锚点
            Vector2 newPosition = Vector2.Lerp(currentAnchorPos, mousePos, dragSpeed * Time.deltaTime);
            dragAnchor.transform.position = newPosition;
        }
    }
    
    protected virtual void UpdatePlacementValidation()
    {
        bool wasInvalid = isInvalidPosition;
        isInvalidPosition = !IsValidPlacement();

        // 更新全局无效放置状态
        if (wasInvalid != isInvalidPosition)
        {
            hasInvalidPlacement = isInvalidPosition;
        }

        if (spriteRenderer != null)
        {
            if (isDragging)
            {
                spriteRenderer.material = draggingMaterial;
            }
            else if (isInvalidPosition)
            {
                spriteRenderer.material = invalidMaterial;
            }
            else
            {
                spriteRenderer.material = normalMaterial;
            }
        }
    }

    // 静态方法检查是否有物体处于无效位置
    public static bool HasInvalidPlacement()
    {
        return hasInvalidPlacement;
    }
    
    // 重置状态
    public static void ResetInvalidPlacement()
    {
        hasInvalidPlacement = false;
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
        
        if (dragJoint != null)
        {
            Destroy(dragJoint);
            dragJoint = null;
        }
        dragAnchor.SetActive(false);
        
        // 设置无效位置物体的状态
        col.isTrigger = true;
        rb.isKinematic = true;  // 改为 true 防止自由落体
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        if (spriteRenderer != null)
        {
            spriteRenderer.material = invalidMaterial;
        }
        
        // 保持其他物体冻结
        FreezeAllObjects();
    }
    
    private void CompletePlace()
    {
        isDragging = false;
        isAnyObjectBeingDragged = false;
        isGlobalFrozen = false;
        isInvalidPosition = false;  // 确保重置无效状态
        
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
        
        // 解冻所有物体
        UnfreezeAllObjects();
    }
    
    protected virtual bool IsValidPlacement()
    {
        // 获取当前区域内的所有碰撞器
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
                // 使用 Distance 获取两个碰撞器之间的最小距离
                ColliderDistance2D distance = Physics2D.Distance(col, otherCol);
                
                // distance.distance 小于 0 表示有重叠
                if (distance.distance <= 0.01f)  // 添加一个小的容差值
                {
                    // 可以输出调试信息
                    Debug.DrawLine(distance.pointA, distance.pointB, Color.red, 0.1f);
                    return false;
                }
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

    // 可以添加一个辅助方法来在Scene视图中显示检测范围（调试用）
    private void OnDrawGizmos()
    {
        if (col != null && isDragging)
        {
            // 显示碰撞器的实际形状
            Gizmos.color = isInvalidPosition ? Color.red : Color.yellow;
            
            if (col is BoxCollider2D boxCol)
            {
                // 为BoxCollider2D绘制旋转后的边界
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
                // 为CircleCollider2D绘制边界
                Gizmos.DrawWireSphere(
                    (Vector2)transform.position + circleCol.offset,
                    circleCol.radius
                );
            }
            // 可以根据需要添加其他类型的碰撞器
        }
    }

    public void TriggerDisappearAnimation()
    {
        // Technical Artist 可以通过这个接口实现消失动画
        if (disappearAnimator != null)
        {
            // animator.SetTrigger("Disappear");  // 动画参数名由 TA 定义
        }
        
        // 1秒后禁用物体
        StartCoroutine(DisableAfterDelay());
    }
    
    private IEnumerator DisableAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
    }
}