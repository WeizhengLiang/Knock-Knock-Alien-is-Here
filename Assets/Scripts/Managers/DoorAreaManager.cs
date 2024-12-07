using UnityEngine;

public class DoorAreaManager : MonoBehaviour
{
    public static DoorAreaManager Instance { get; private set; }
    
    [Header("区域设置")]
    [SerializeField] private DoorCoverageCalculator doorCoverageCalculator;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public float GetCurrentCoverage()
    {
        return doorCoverageCalculator != null ? doorCoverageCalculator.GetCurrentCoverage() : 0f;
    }

    private void OnDestroy() 
    {
        if (Instance == this) 
        {
            Instance = null;
        }
    }
}