using UnityEngine;

public class FragileObject : DraggableObject
{
    [Header("易碎物体属性")]
    [SerializeField] private float fragileStrength = 0.5f;
    [SerializeField] private Color warningColor = new Color(1f, 0.5f, 0f); // 橙色警告
    
    protected override void Start()
    {
        base.Start();
        // 易碎物体可能需要更快的拖拽速度来体现轻盈感
        dragSpeed *= 1.2f;
    }
    
    public override bool IsFragile() { return true; }
    public override float GetStrength() { return fragileStrength; }
    
    protected override void UpdatePlacementValidation()
    {
        bool isValid = IsValidPlacement();
        if (!isValid)
        {
            // 易碎物体在无效位置时显示警告色
            spriteRenderer.color = warningColor;
        }
        else
        {
            spriteRenderer.color = originalColor;
        }
        isInvalidPosition = !isValid;
    }
}