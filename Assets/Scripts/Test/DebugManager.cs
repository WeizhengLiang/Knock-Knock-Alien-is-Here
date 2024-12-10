using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public static DebugManager Instance { get; private set; }
    private Vector2 scrollPosition;
    private bool showCollectibleButtons = false;
    private bool showSoundDebug = false;

    private void Awake()
    {
        // 单例模式设置
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    private void OnGUI()
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        GUILayout.BeginArea(new Rect(10, 10, 200, 500));
        
        // 显示当前场景名称
        GUILayout.Label($"当前场景: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}", GUI.skin.box);
        // 主要功能按钮
        if (GUILayout.Button("清除所有收集物数据"))
        {
            PlayerPrefsManager.ClearAllCollectibles();
        }
        
        if (GUILayout.Button("解锁所有收集物"))
        {
            PlayerPrefsManager.UnlockAllCollectibles();
        }

        // 添加性能监控选项
        if (GUILayout.Button("切换内存监视器"))
        {
            PerformanceMonitorManager.Instance?.ToggleMemoryMonitor(
                !(PerformanceMonitorManager.Instance.GetComponent<MemoryMonitor>()?.enabled ?? false)
            );
        }
        
        if (GUILayout.Button("强制内存检查"))
        {
            PerformanceMonitorManager.Instance?.ForceMemoryCheck();
        }

        // 音效调试开关
        showSoundDebug = GUILayout.Toggle(showSoundDebug, "显示当前播放的音效");
        
        if (showSoundDebug && SoundManager.Instance != null)
        {
            GUILayout.Label("当前播放的音效：", GUI.skin.box);
            var playingSounds = SoundManager.Instance.GetPlayingSounds();
            
            if (playingSounds.Count == 0)
            {
                GUILayout.Label("没有正在播放的音效");
            }
            else
            {
                foreach (string sound in playingSounds)
                {
                    GUILayout.Label(sound, GUI.skin.box);
                }
            }
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
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}