using UnityEngine;

public class HeavyObject : DraggableObject
{
    [Header("重物属性")]
    [SerializeField] private float heavyWeight = 5f;
    [SerializeField] private float heavyStrength = 3f;
    
    protected override void Start()
    {
        base.Start();
        // 重物可能需要更大的拖拽速度来体现重量感
        baseDragSpeed *= 0.8f;
        CalculateDragSpeed();  // 重新计算实际拖拽速度
        
        // 增加重力影响
        if (rb != null)
        {
            // 重物有额外的重力加成
            rb.gravityScale *= 1.3f;  // 可以调整这个系数来改变重物的下落速度
        }
    }
    
    public override float GetWeight() { return heavyWeight; }
    public override float GetStrength() { return heavyStrength; }
}