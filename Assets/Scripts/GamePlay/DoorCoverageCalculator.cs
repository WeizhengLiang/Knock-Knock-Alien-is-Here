using UnityEngine;

public class DoorCoverageCalculator : MonoBehaviour
{
    [Header("判定设置")]
    [SerializeField] private float requiredCoveragePercent = 90f;  // 需要遮挡的百分比
    [SerializeField] private LayerMask coverageLayer;              // 用于检测的层级
    [SerializeField] private Vector2 doorSize;                     // 门的尺寸
    [SerializeField] private int raycastCount = 20;               // 射线检测的数量
    [SerializeField] private float raycastDistance = 0.1f;        // 射线检测距离
    
    private float currentCoverage = 0f;                           // 当前遮挡百分比
    private bool isLevelComplete = false;
    
    private void FixedUpdate()
    {
        // 只在游戏进行中计算遮挡
        if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            CalculateCoverage();
        }
    }
    
    private void CalculateCoverage()
    {
        int hitCount = 0;
        Vector2 startPos = transform.position;
        
        // 计算门的边界
        float halfWidth = doorSize.x / 2f;
        float halfHeight = doorSize.y / 2f;
        
        // 创建射线网格
        for (int x = 0; x < raycastCount; x++)
        {
            for (int y = 0; y < raycastCount; y++)
            {
                // 计算射线起点
                Vector2 rayStart = new Vector2(
                    startPos.x - halfWidth + (doorSize.x * x / (raycastCount - 1)),
                    startPos.y - halfHeight + (doorSize.y * y / (raycastCount - 1))
                );
                
                // 发射射线
                RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.right, raycastDistance, coverageLayer);
                // 只有当碰撞到的物体不是无效放置且不在拖动状态时才计数
                if (hit.collider != null)
                {
                    DraggableObject obj = hit.collider.GetComponent<DraggableObject>();
                    if (obj != null && !obj.IsInInvalidPosition() && !obj.IsDragging())
                    {
                        hitCount++;
                    }
                }
                
                // 可视化射线（仅在编辑器中）
                Debug.DrawRay(rayStart, Vector2.right * raycastDistance, hit.collider != null ? Color.green : Color.red);
            }
        }
        
        // 计算覆盖百分比
        currentCoverage = (hitCount * 100f) / (raycastCount * raycastCount);
        
        // 输出调试信息
        Debug.Log($"当前遮挡率: {currentCoverage}%");
    }
    
    private void CheckWinCondition()
    {
        if (currentCoverage >= requiredCoveragePercent && !isLevelComplete)
        {
            isLevelComplete = true;
            OnLevelComplete();
        }
    }
    
    private void OnLevelComplete()
    {
        Debug.Log("关卡完成！");
        // 通知游戏管理器
        GameManager.Instance.OnLevelComplete();
    }
    
    // 在编辑器中绘制门的范围
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(doorSize.x, doorSize.y, 0.1f));
    }
    
    // 获取当前遮挡率
    public float GetCurrentCoverage()
    {
        return currentCoverage;
    }
}