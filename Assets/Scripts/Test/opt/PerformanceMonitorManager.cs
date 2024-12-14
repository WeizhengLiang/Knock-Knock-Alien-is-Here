using UnityEngine;

public class PerformanceMonitorManager : MonoBehaviour
{
    public static PerformanceMonitorManager Instance { get; private set; }
    private MemoryMonitor memoryMonitor;
    private PerformanceWarning performanceWarning;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeMonitors();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeMonitors()
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        // 添加内存监视器
        if (memoryMonitor == null)
        {
            memoryMonitor = gameObject.AddComponent<MemoryMonitor>();
        }
        
        // 添加性能警告系统
        if (performanceWarning == null)
        {
            performanceWarning = gameObject.AddComponent<PerformanceWarning>();
        }
        #endif
    }

    // 提供开关控制
    public void ToggleMemoryMonitor(bool enable)
    {
        if (memoryMonitor != null)
        {
            memoryMonitor.enabled = enable;
        }
    }

    public void TogglePerformanceWarning(bool enable)
    {
        if (performanceWarning != null)
        {
            performanceWarning.enabled = enable;
        }
    }

    // 手动触发内存检查
    public void ForceMemoryCheck()
    {
        if (performanceWarning != null)
        {
            performanceWarning.SendMessage("CheckPerformance", 0f);
        }
    }

    private void Update()
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        // F1 切换内存监视器
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ToggleMemoryMonitor(!memoryMonitor.enabled);
        }
        
        // F2 切换性能警告
        if (Input.GetKeyDown(KeyCode.F2))
        {
            TogglePerformanceWarning(!performanceWarning.enabled);
        }
        
        // F3 强制内存检查
        if (Input.GetKeyDown(KeyCode.F3))
        {
            ForceMemoryCheck();
        }
        #endif
    }
}