using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Calculates the coverage percentage of a door area using raycasts
/// Used to determine if enough objects are blocking the door
/// </summary>
public class DoorCoverageCalculator : MonoBehaviour
{
    #region Serialized Fields
    [Header("Detection Settings")]
    [SerializeField] private float requiredCoveragePercent = 90f;
    [SerializeField] private LayerMask draggableLayer;
    [SerializeField] private LayerMask coverageLayer;
    [SerializeField] private Vector2 doorSize;
    [SerializeField] private int raycastCount = 20;
    [SerializeField] private float raycastDistance = 0.1f;
    [SerializeField] public BoxCollider2D doorArea;
    #endregion

    #region Private Fields
    private float currentCoverage = 0f;
    private bool isLevelComplete = false;
    private Vector2[] raycastPoints;  // 缓存射线检测点
    private float raycastGridSize;    // 每个格子的大小
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        // 预计算所有射线检测点
        InitializeRaycastPoints();
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            CalculateCoverage();
        }
    }
    #endregion

    #region Coverage Calculation
    private void InitializeRaycastPoints()
    {
        raycastPoints = new Vector2[raycastCount * raycastCount];
        raycastGridSize = doorSize.x / (raycastCount - 1);
        Vector2 startPos = (Vector2)transform.position - doorSize / 2f;

        for (int x = 0; x < raycastCount; x++)
        {
            for (int y = 0; y < raycastCount; y++)
            {
                raycastPoints[x * raycastCount + y] = new Vector2(
                    startPos.x + (doorSize.x * x / (raycastCount - 1)),
                    startPos.y + (doorSize.y * y / (raycastCount - 1))
                );
            }
        }
    }

    private void CalculateCoverage()
    {
        int hitCount = 0;
        int totalPoints = raycastPoints.Length;
        
        // 合并两个层的遮罩
        LayerMask combinedMask = draggableLayer | coverageLayer;

        for (int i = 0; i < totalPoints; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(raycastPoints[i], Vector2.right, raycastDistance, combinedMask);
            
            if (hit.collider != null)
            {
                // 先检查是否是可拖拽物体
                DraggableObject obj = hit.collider.GetComponent<DraggableObject>();
                if (obj != null)
                {
                    // 如果是可拖拽物体，检查其状态
                    if (!obj.IsInvalidPosition && !obj.IsDragging)
                    {
                        hitCount++;
                    }
                }
                else
                {
                    // 如果不是可拖拽物体（比如碎片），直接计入覆盖
                    hitCount++;
                }
            }

            #if UNITY_EDITOR
            Debug.DrawRay(raycastPoints[i], Vector2.right * raycastDistance, hit.collider != null ? Color.green : Color.red);
            #endif
        }

        currentCoverage = (hitCount * 100f) / totalPoints;
    }
    #endregion

    #region Public Methods
    public float GetCurrentCoverage()
    {
        return currentCoverage;
    }
    #endregion

    #region Debug Visualization
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(doorSize.x, doorSize.y, 0.1f));
    }
    #endregion
}