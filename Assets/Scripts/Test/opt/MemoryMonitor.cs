using UnityEngine;
using UnityEngine.Profiling;

public class MemoryMonitor : MonoBehaviour
{
    private bool showMemoryStats = false;
    private GUIStyle style;
    private float updateInterval = 0.5f;
    private float lastUpdateTime;

    private void Awake()
    {
        style = new GUIStyle
        {
            fontSize = 20,
            normal = { textColor = Color.white }
        };
        
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        showMemoryStats = true;
        #endif
    }

    private void OnGUI()
    {
        if (!showMemoryStats) return;

        if (Time.realtimeSinceStartup - lastUpdateTime >= updateInterval)
        {
            lastUpdateTime = Time.realtimeSinceStartup;
        }

        float x = 10;
        float y = 50;
        float width = 300;
        float height = 250;

        GUI.Box(new Rect(x, y, width, height), "");

        string stats = string.Format(
            "系统总内存: {0:F2} MB\n" +
            "已分配内存: {1:F2} MB\n" +
            "未使用内存: {2:F2} MB\n" +
            "单帧内存分配: {3:F2} KB\n" +
            "GC.收集次数: {4}\n" +
            "贴图内存: {5:F2} MB\n" +
            "网格内存: {6:F2} MB\n" +
            "音频内存: {7:F2} MB\n" +
            "总内存使用: {8:F2} MB",
            SystemInfo.systemMemorySize / 1024f,
            System.GC.GetTotalMemory(false) / 1024f / 1024f,
            SystemInfo.systemMemorySize / 1024f - System.GC.GetTotalMemory(false) / 1024f / 1024f,
            UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong() / 1024f,
            System.GC.CollectionCount(0),
            GetObjectsMemoryUsage<Texture>() / 1024f / 1024f,
            GetObjectsMemoryUsage<Mesh>() / 1024f / 1024f,
            GetObjectsMemoryUsage<AudioClip>() / 1024f / 1024f,
            Profiler.GetTotalAllocatedMemoryLong() / 1024f / 1024f
        );

        GUI.Label(new Rect(x + 10, y + 10, width - 20, height - 20), stats, style);

        // 添加切换按钮
        if (GUI.Button(new Rect(x, y - 30, 100, 25), "切换显示"))
        {
            showMemoryStats = !showMemoryStats;
        }

        // 添加GC按钮
        if (GUI.Button(new Rect(x + 110, y - 30, 100, 25), "强制GC"))
        {
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
        }
    }

    // 获取特定对象类型的内存使用
    private long GetObjectsMemoryUsage<T>() where T : UnityEngine.Object
    {
        long totalMemory = 0;
        T[] objects = Resources.FindObjectsOfTypeAll<T>();
        foreach (T obj in objects)
        {
            totalMemory += Profiler.GetRuntimeMemorySizeLong(obj);
        }
        return totalMemory;
    }

    // 检测内存泄漏
    private void CheckForMemoryLeaks()
    {
        float currentMemory = System.GC.GetTotalMemory(false);
        System.GC.Collect();
        float afterGCMemory = System.GC.GetTotalMemory(true);
        
        if ((currentMemory - afterGCMemory) > 50 * 1024 * 1024) // 如果差异超过50MB
        {
            Debug.LogWarning($"可能存在内存泄漏! 垃圾回收前: {currentMemory/1024/1024:F2}MB, 回收后: {afterGCMemory/1024/1024:F2}MB");
        }
    }

    // 定期检查内存（可选）
    private void Update()
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Time.frameCount % 300 == 0) // 每300帧检查一次
        {
            CheckForMemoryLeaks();
        }
        #endif
    }
}