using UnityEngine;
using System.Collections.Generic;

public class FragileObject : DraggableObject
{
    [Header("易碎物体属性")]
    [SerializeField] private float fragileStrength = 0.5f;
    
    [Header("破碎条件")]
    [SerializeField] private float breakImpactThreshold = 10f;  // 瞬时冲击阈值
    [SerializeField] private float breakForceThreshold = 15f;   // 持续受力阈值
    [SerializeField] private float forceResetTime = 0.5f;       // 重置累积力的时间
    
    [Header("破碎效果")]
    [SerializeField] public GameObject brokenPrefab;           // 破碎预制体

    private float accumulatedForce = 0f;    // 累积受力
    private float lastForceTime;            // 上次受力时间
    private bool isBroken = false;          // 是否已破碎
    
    private static List<GameObject> brokenPieces = new List<GameObject>();
    
    protected override void Start()
    {
        base.Start();
        // 易碎物体可能需要更快的拖拽速度来体现轻盈感
        baseDragSpeed *= 1.2f;
        CalculateDragSpeed();  // 重新计算实际拖拽速度
        lastForceTime = Time.time;
    }
    
    public override bool IsFragile() { return true; }
    public override float GetStrength() { return fragileStrength; }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isBroken || isDragging) return;  // 拖拽时不检测破碎
        
        // 计算冲击力（考虑质量和速度）
        float impactForce = collision.relativeVelocity.magnitude * 
            (collision.rigidbody?.mass ?? 1f);
            
        // 输出调试信息
        Debug.Log($"Impact force: {impactForce} on {gameObject.name}");
        
        // 检查是否超过瞬时破碎阈值
        if (impactForce >= breakImpactThreshold)
        {
            Break(collision.GetContact(0).point);
        }
    }
    
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isBroken || isDragging) return;  // 拖拽时不检测破碎
        
        // 只检查来自上方的碰撞
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // 如果接触点在物体上方，且有其他物体在上面
            if (contact.point.y > transform.position.y && 
                collision.gameObject.transform.position.y > transform.position.y)
            {
                // 计算上方物体施加的压力（仅考虑物体质量）
                float appliedForce = (collision.rigidbody?.mass ?? 1f) * Physics2D.gravity.magnitude;
                
                // 检查时间间隔，决定是累积还是重置
                float currentTime = Time.time;
                if (currentTime - lastForceTime > forceResetTime)
                {
                    accumulatedForce = 0f;
                }
                
                // 累积受力
                accumulatedForce += appliedForce * Time.deltaTime;
                lastForceTime = currentTime;
                
                // 输出调试信息
                Debug.Log($"Pressure force: {accumulatedForce} on {gameObject.name} from {collision.gameObject.name}");
                
                // 检查是否超过持续受力阈值
                if (accumulatedForce >= breakForceThreshold)
                {
                    Break(contact.point);
                }
                
                break; // 找到一个有效的压力点就退出循环
            }
        }
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        // 当上方物体离开时，重置累积力
        if (collision.gameObject.transform.position.y > transform.position.y)
        {
            accumulatedForce = 0f;
            lastForceTime = Time.time;
        }
    }
    
    private void Break(Vector2 breakPoint)
    {
        if (isBroken) return;
        isBroken = true;
        
        Debug.Log($"{gameObject.name}已破碎");
        
        if (brokenPrefab != null)
        {
            GameObject broken = Instantiate(brokenPrefab, transform.position, transform.rotation);
            brokenPieces.Add(broken); // 添加到列表中追踪
            
            // 如果破碎模型有Rigidbody2D，传递原物体的速度
            if (rb != null && broken.TryGetComponent<Rigidbody2D>(out var brokenRb))
            {
                brokenRb.velocity = rb.velocity;
                brokenRb.angularVelocity = rb.angularVelocity;
            }
        }
        
        Destroy(gameObject);
    }
    
    // 用于调试的可视化
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && accumulatedForce > 0)
        {
            float radius = accumulatedForce / breakForceThreshold * 0.5f;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();  // 调用基类的OnDestroy
        
        // 清理引用
        brokenPrefab = null;
        isBroken = false;
        accumulatedForce = 0f;
    }

    // 添加静态清理方法
    public static void CleanupBrokenPieces()
    {
        foreach (var piece in brokenPieces)
        {
            if (piece != null)
            {
                Destroy(piece);
            }
        }
        brokenPieces.Clear();
    }
}