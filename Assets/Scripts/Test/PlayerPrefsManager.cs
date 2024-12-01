using UnityEngine;

public static class PlayerPrefsManager
{
    // 清除所有收集物相关的PlayerPrefs数据
    public static void ClearAllCollectibles()
    {
        // 清除所有收集物解锁状态
        foreach (CollectibleType type in System.Enum.GetValues(typeof(CollectibleType)))
        {
            PlayerPrefs.DeleteKey($"Collectible_{type}");
        }
        
        // 清除新收集物标记
        PlayerPrefs.DeleteKey("HasNewCollectible");
        
        PlayerPrefs.Save();
        Debug.Log("已清除所有收集物数据");
        
        // 如果CollectibleManager存在，重新初始化
        if (CollectibleManager.Instance != null)
        {
            CollectibleManager.Instance.InitializeCollection();
        }
    }

    // 解锁所有收集物（用于测试）
    public static void UnlockAllCollectibles()
    {
        foreach (CollectibleType type in System.Enum.GetValues(typeof(CollectibleType)))
        {
            PlayerPrefs.SetInt($"Collectible_{type}", 1);
        }
        
        PlayerPrefs.SetInt("HasNewCollectible", 1);
        PlayerPrefs.Save();
        Debug.Log("已解锁所有收集物");
        
        // 如果CollectibleManager存在，重新初始化
        if (CollectibleManager.Instance != null)
        {
            CollectibleManager.Instance.InitializeCollection();
        }
    }

    // 解锁单个收集物
    public static void UnlockCollectible(CollectibleType type)
    {
        PlayerPrefs.SetInt($"Collectible_{type}", 1);
        PlayerPrefs.SetInt("HasNewCollectible", 1);
        PlayerPrefs.Save();
        Debug.Log($"已解锁收集物: {type}");
        
        // 如果CollectibleManager存在，重新初始化
        if (CollectibleManager.Instance != null)
        {
            CollectibleManager.Instance.InitializeCollection();
        }
    }

    // 锁定单个收集物
    public static void LockCollectible(CollectibleType type)
    {
        PlayerPrefs.SetInt($"Collectible_{type}", 0);
        PlayerPrefs.Save();
        Debug.Log($"已锁定收集物: {type}");
        
        // 如果CollectibleManager存在，重新初始化
        if (CollectibleManager.Instance != null)
        {
            CollectibleManager.Instance.InitializeCollection();
        }
    }
}