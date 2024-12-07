using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    public enum SceneType
    {
        MainMenu,       // 开始界面
        OpeningComic,   // 开场漫画
        GameScene,      // 游戏主界面
        Collection,     // 收藏品界面
        WinAnimation,   // 胜利动画
        LoseAnimation   // 失败动画
    }

    [System.Serializable]
    private class SceneData
    {
        public SceneType sceneType;
        public string sceneName;
    }

    [Header("Scene Settings")]
    [SerializeField] private SceneData[] sceneData;
    [SerializeField] private float transitionTime = 0f;
    
    private Dictionary<SceneType, string> sceneMap;
    private SceneType previousScene;  // 记录进入Collection前的场景
    private bool isReturningFromCollection = false;  // 标记是否从Collection返回

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSceneMap();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSceneMap()
    {
        sceneMap = new Dictionary<SceneType, string>();
        foreach (var data in sceneData)
        {
            sceneMap[data.sceneType] = data.sceneName;
        }
    }

    // 核心场景跳转方法
    private void LoadScene(SceneType sceneType)
    {
        if (sceneMap.TryGetValue(sceneType, out string sceneName))
        {
            GameManager.Instance.CleanupBeforeSceneChange();
            StartCoroutine(LoadSceneRoutine(sceneName));
        }
        else
        {
            Debug.LogError($"Scene type {sceneType} not found in scene map!");
        }
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        // 这里可以添加转场动画
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(sceneName);
    }

    #region 场景跳转公共方法
    
    // 开始界面 -> 开场漫画
    public void StartGameFromMenu()
    {
        LoadScene(SceneType.OpeningComic);
    }
    
    // 开场漫画 -> 游戏主界面
    public void StartGameFromComic()
    {
        LoadScene(SceneType.GameScene);
    }
    
    // 游戏主界面 -> 开始界面
    public void ReturnToMainMenu()
    {
        StartCoroutine(LoadMainMenuWithUnlockCheck());
    }
    
    // 游戏主界面 -> 胜利动画
    public void ShowWinAnimation()
    {
        LoadScene(SceneType.WinAnimation);
    }
    
    // 游戏主界面 -> 失败动画
    public void ShowLoseAnimation()
    {
        LoadScene(SceneType.LoseAnimation);
    }
    
    // 胜利/失败动画 -> 开场漫画
    public void ReturnToComic()
    {
        LoadScene(SceneType.OpeningComic);
    }
    
    // 从主菜单进入收藏品界面
    public void EnterCollection()
    {
        PlayerPrefs.SetInt("HasNewCollectible", 0);  // 清除新收藏品标记
        LoadScene(SceneType.Collection);
    }
    
    // 从收藏品界面返回主菜单
    public void ExitCollection()
    {
        LoadScene(SceneType.MainMenu);
    }
    
    #endregion

    private void OnDestroy()
    {
        StopAllCoroutines();
        if (Instance == this)
        {
            Instance = null;
        }
        // 确保在场景切换时也清理
        DraggableObject.ClearStaticReferences();
        // 其他管理器的清理...
    }

    // 当加载到MainScene时调用
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 只处理进入GameScene的情况
        if (scene.name == sceneMap[SceneType.GameScene])
        {
            GameManager.Instance?.EnterMainScene();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        // 清理所有静态引用
        DraggableObject.ClearStaticReferences();
        // 其他管理器的清理...
    }

    public void ShowWinScene()
    {
        StartCoroutine(LoadSceneWithUnlockCheck("WinScene"));
    }
    
    public void ShowLoseScene()
    {
        StartCoroutine(LoadSceneWithUnlockCheck("LoseScene"));
    }
    
    private IEnumerator LoadSceneWithUnlockCheck(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // 场景加载完成后，显示解锁消息和图标
        var unlockMessageUI = FindObjectOfType<UnlockMessageUI>();
        unlockMessageUI?.ShowUnlockMessage();
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateNewCollectibleIcons();
        }
    }
    
    private IEnumerator LoadMainMenuWithUnlockCheck()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainMenuScene");
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateNewCollectibleIcons();
        }
    }
}