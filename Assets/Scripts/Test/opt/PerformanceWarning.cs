using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PerformanceWarning : MonoBehaviour
{
    private const float MEMORY_WARNING_THRESHOLD = 1000f; // MB
    private const float FPS_WARNING_THRESHOLD = 30f;
    private float deltaTime = 0f;
    private float warningInterval = 5f;
    private float lastWarningTime = 0f;

    private void Update()
    {
        // 更新FPS计算
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;

        if (Time.time - lastWarningTime >= warningInterval)
        {
            CheckPerformance(fps);
            lastWarningTime = Time.time;
        }
    }

    private void CheckPerformance(float fps)
    {
        // 检查内存使用
        float totalMemoryMB = System.GC.GetTotalMemory(false) / 1024f / 1024f;
        if (totalMemoryMB > MEMORY_WARNING_THRESHOLD)
        {
            Debug.LogWarning($"内存使用过高: {totalMemoryMB:F2}MB");
            LogMemoryHogs();
        }

        // 检查FPS
        if (fps < FPS_WARNING_THRESHOLD)
        {
            Debug.LogWarning($"FPS过低: {fps:F1}");
        }
    }

    private void LogMemoryHogs()
    {
        var objects = FindObjectsOfType<UnityEngine.Object>();
        var memoryHogs = new List<(string name, long memory)>();

        foreach (var obj in objects)
        {
            long memory = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(obj);
            if (memory > 1024 * 1024) // 大于1MB的对象
            {
                memoryHogs.Add((obj.name, memory));
            }
        }

        // 按内存使用量排序
        memoryHogs.Sort((a, b) => b.memory.CompareTo(a.memory));

        // 输出前10个内存占用最大的对象
        foreach (var hog in memoryHogs.Take(10))
        {
            Debug.LogWarning($"大内存对象: {hog.name} - {hog.memory/1024f/1024f:F2}MB");
        }
    }
}