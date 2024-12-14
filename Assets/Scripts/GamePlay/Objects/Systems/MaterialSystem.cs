using UnityEngine;

public class MaterialSystem : IGameSystem
{
    private readonly DraggableObject owner;
    private readonly SpriteRenderer spriteRenderer;
    
    private Material normalMaterial;
    private Material draggingMaterial;
    private Material invalidMaterial;
    private Material canMergeMaterial;
    private Material canPickupMaterial;
    
    private int defaultSortingOrder;
    private int interactiveSortingOrder;

    public MaterialSystem(DraggableObject owner)
    {
        this.owner = owner;
        this.spriteRenderer = owner.GetComponent<SpriteRenderer>();
        
        // 初始化材质设置
        normalMaterial = owner.NormalMaterial;
        draggingMaterial = owner.DraggingMaterial;
        invalidMaterial = owner.InvalidMaterial;
        canMergeMaterial = owner.CanMergeMaterial;
        canPickupMaterial = owner.CanPickupMaterial;
        
        defaultSortingOrder = owner.DefaultSortingOrder;
        interactiveSortingOrder = owner.InteractiveSortingOrder;

        // 订阅事件
        DraggableObjectEvents.OnDragStateChanged += HandleDragStateChanged;
        DraggableObjectEvents.OnValidStateChanged += HandleValidStateChanged;
    }

    public void Cleanup()
    {
        // 取消事件订阅
        DraggableObjectEvents.OnDragStateChanged -= HandleDragStateChanged;
        DraggableObjectEvents.OnValidStateChanged -= HandleValidStateChanged;
    }

    private void HandleDragStateChanged(DraggableObject obj, bool isDragging)
    {
        if (obj != owner) return;
        UpdateDragState();
    }

    private void HandleValidStateChanged(DraggableObject obj, bool isValid)
    {
        if (obj != owner) return;
        UpdateMaterial();
    }

    public void UpdateDragState()
    {
        if (spriteRenderer == null) return;
        
        spriteRenderer.sortingOrder = owner.IsDragging ? interactiveSortingOrder : defaultSortingOrder;
        UpdateMaterial();
    }

    public void UpdateMaterial()
    {
        if (spriteRenderer == null) return;

        if (owner.IsDragging)
        {
            spriteRenderer.material = owner.IsInvalidPosition ? invalidMaterial : draggingMaterial;
        }
        else if (owner.IsInvalidPosition)
        {
            spriteRenderer.material = invalidMaterial;
        }
        else if (owner.IsMouseOver)
        {
            spriteRenderer.material = canPickupMaterial;
        }
        else
        {
            spriteRenderer.material = normalMaterial;
        }
    }

    public void Initialize()
    {
        // 订阅事件已经在构造函数中完成
    }

    public void Update()
    {
        UpdateMaterial();
    }

    public void FixedUpdate() { }
} 