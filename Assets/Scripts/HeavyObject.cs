using UnityEngine;

public class HeavyObject : DraggableObject
{
    [Header("重物属性")]
    [SerializeField] private float heavyWeight = 5f;
    [SerializeField] private float heavyStrength = 3f;
    [SerializeField] private float rotationSpeed = 5f; 
    
    protected override void Start()
    {
        base.Start();
        // 重物可能需要更大的拖拽速度来体现重量感
        dragSpeed *= 0.8f;
    }
    
    public override float GetWeight() { return heavyWeight; }
    public override float GetStrength() { return heavyStrength; }
}