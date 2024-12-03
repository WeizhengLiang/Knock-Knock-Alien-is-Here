using UnityEngine;

/// <summary>
/// Calculates the coverage percentage of a door area using raycasts
/// Used to determine if enough objects are blocking the door
/// </summary>
public class DoorCoverageCalculator : MonoBehaviour
{
    #region Serialized Fields
    [Header("Detection Settings")]
    [SerializeField] private float requiredCoveragePercent = 90f;  // Required coverage percentage to complete level
    [SerializeField] private LayerMask coverageLayer;              // Layer mask for raycast detection
    [SerializeField] private Vector2 doorSize;                     // Size of the door area
    [SerializeField] private int raycastCount = 20;                // Number of raycasts per axis
    [SerializeField] private float raycastDistance = 0.1f;         // Distance of each raycast
    #endregion

    #region Private Fields
    private float currentCoverage = 0f;                            // Current coverage percentage
    private bool isLevelComplete = false;                          // Level completion state
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// Calculates coverage during fixed time intervals
    /// Only active during gameplay
    /// </summary>
    private void FixedUpdate()
    {
        if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            CalculateCoverage();
        }
    }
    #endregion

    #region Coverage Calculation
    /// <summary>
    /// Calculates the current coverage percentage using a grid of raycasts
    /// </summary>
    private void CalculateCoverage()
    {
        int hitCount = 0;
        Vector2 startPos = transform.position;
        
        // Calculate door boundaries
        float halfWidth = doorSize.x / 2f;
        float halfHeight = doorSize.y / 2f;
        
        // Create raycast grid
        for (int x = 0; x < raycastCount; x++)
        {
            for (int y = 0; y < raycastCount; y++)
            {
                // Calculate raycast start position
                Vector2 rayStart = new Vector2(
                    startPos.x - halfWidth + (doorSize.x * x / (raycastCount - 1)),
                    startPos.y - halfHeight + (doorSize.y * y / (raycastCount - 1))
                );
                
                // Cast ray
                RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.right, raycastDistance, coverageLayer);
                
                // Count valid hits (objects not in invalid position or being dragged)
                if (hit.collider != null)
                {
                    DraggableObject obj = hit.collider.GetComponent<DraggableObject>();
                    if (obj != null && !obj.IsInInvalidPosition() && !obj.IsDragging())
                    {
                        hitCount++;
                    }
                }
                
                // Visualize rays (editor only)
                Debug.DrawRay(rayStart, Vector2.right * raycastDistance, hit.collider != null ? Color.green : Color.red);
            }
        }
        
        // Calculate coverage percentage
        currentCoverage = (hitCount * 100f) / (raycastCount * raycastCount);
        
        // Debug output
        Debug.Log($"Current Coverage: {currentCoverage}%");
    }
    #endregion

    #region Level Completion
    /// <summary>
    /// Checks if coverage meets win condition
    /// </summary>
    private void CheckWinCondition()
    {
        if (currentCoverage >= requiredCoveragePercent && !isLevelComplete)
        {
            isLevelComplete = true;
            OnLevelComplete();
        }
    }

    /// <summary>
    /// Handles level completion
    /// </summary>
    private void OnLevelComplete()
    {
        Debug.Log("Level Complete!");
        GameManager.Instance.OnLevelComplete();
    }
    #endregion

    #region Debug Visualization
    /// <summary>
    /// Draws door area in editor
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(doorSize.x, doorSize.y, 0.1f));
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Gets current coverage percentage
    /// </summary>
    public float GetCurrentCoverage()
    {
        return currentCoverage;
    }
    #endregion
}