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
        LoadScene(SceneType.MainMenu);
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
    
    #endregion

    private void OnDisable()
    {
        // 清理所有静态引用
        DraggableObject.ClearStaticReferences();
        // 其他管理器的清理...
    }

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
}