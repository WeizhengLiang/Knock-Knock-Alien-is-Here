using UnityEngine;

public class DebugManager : MonoBehaviour
{
    private Vector2 scrollPosition;
    private bool showCollectibleButtons = false;

    private void OnGUI()
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        GUILayout.BeginArea(new Rect(10, 10, 200, 400));
        
        // 主要功能按钮
        if (GUILayout.Button("清除所有收集物数据"))
        {
            PlayerPrefsManager.ClearAllCollectibles();
        }
        
        if (GUILayout.Button("解锁所有收集物"))
        {
            PlayerPrefsManager.UnlockAllCollectibles();
        }

        // 单个收集物解锁按钮
        showCollectibleButtons = GUILayout.Toggle(showCollectibleButtons, "单个收集物解锁");
        
        if (showCollectibleButtons)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            
            foreach (CollectibleType type in System.Enum.GetValues(typeof(CollectibleType)))
            {
                bool isUnlocked = CollectibleManager.Instance?.IsCollectibleUnlocked(type) ?? false;
                string buttonText = $"{type} [{(isUnlocked ? "已解锁" : "未解锁")}]";
                
                if (GUILayout.Button(buttonText))
                {
                    if (isUnlocked)
                    {
                        PlayerPrefsManager.LockCollectible(type);
                    }
                    else
                    {
                        PlayerPrefsManager.UnlockCollectible(type);
                    }
                }
            }
            
            GUILayout.EndScrollView();
        }
        
        GUILayout.EndArea();
        #endif
    }
}