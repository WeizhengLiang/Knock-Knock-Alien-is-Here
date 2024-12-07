using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    // 游戏状态枚举
    public enum GameState
    {
        Menu,       // MainMenuScene
        Ready,      // MainScene - 准备开始状态
        Playing,    // MainScene - 游戏进行中
        GameOver    // MainScene - 游戏结束
    }
    
    // 游戏状态变量
    public GameState CurrentState { get; private set; }
    private float remainingTime;

    [Header("游戏设置")]
    [SerializeField] private float levelCompleteDelay = 1f;
    [SerializeField] private float totalGameTime = 20f;
    [SerializeField] private float finalCountdownTime = 3f;
    [SerializeField] private float winThreshold = 70f;
    [SerializeField] private float startCountdownTime = 3f;  // 开始倒计时时间
    private bool isFinalCountdown = false;
    private bool isStartCountdown = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // 游戏开始时就锁定相机和交互
        SmoothCameraScroller.Instance?.SetCameraLocked(true);
        DraggableObject.SetGlobalFrozen(true);
        SetGameState(GameState.Menu);
    }
    
    // 当加载到MainScene时调用
    public void EnterMainScene()
    {
        // 进入主场景时保持锁定状态
        SmoothCameraScroller.Instance?.SetCameraLocked(true);
        DraggableObject.SetGlobalFrozen(true);
        SetGameState(GameState.Ready);
    }
    
    private void Update()
    {
        if (CurrentState == GameState.Playing)
        {
            UpdateGameTimer();
        }
    }
    
    private void UpdateGameTimer()
    {
        remainingTime -= Time.deltaTime;
        
        if (!isFinalCountdown && remainingTime <= finalCountdownTime)
        {
            isFinalCountdown = true;
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowFinalCountdown();
            }
        }
        
        if (remainingTime <= 0)
        {
            EndGame();
        }
    }
    
    public void StartGame()
    {
        if (CurrentState != GameState.Ready) return;
        
        // 先切换UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SwitchToGamePanel();
        }
        
        isStartCountdown = true;
        StartCoroutine(StartCountdownSequence());
    }
    
    private IEnumerator StartCountdownSequence()
    {
        // 保持锁定状态
        SmoothCameraScroller.Instance?.SetCameraLocked(true);
        DraggableObject.SetGlobalFrozen(true);
        
        // 显示倒计时
        if (UIManager.Instance != null)
        {
            yield return UIManager.Instance.ShowStartCountdownAndWait();
        }
        
        // 倒计时结束，解锁并开始游戏
        SmoothCameraScroller.Instance?.SetCameraLocked(false);
        DraggableObject.SetGlobalFrozen(false);
        
        // 开始正式游戏
        remainingTime = totalGameTime;
        isFinalCountdown = false;
        isStartCountdown = false;
        SetGameState(GameState.Playing);
    }
    
    public void EndGame()
    {
        remainingTime = 0;
        SetGameState(GameState.GameOver);
        OnCountdownComplete();
    }

    public void RestartGame()
    {
        // 重启时重新锁定
        SmoothCameraScroller.Instance?.SetCameraLocked(true);
        DraggableObject.SetGlobalFrozen(true);
        
        remainingTime = totalGameTime;
        Time.timeScale = 1;
        DraggableObject.ResetGlobalState();
        
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
        
        SetGameState(GameState.Ready);
    }
    
    private void SetGameState(GameState newState)
    {
        CurrentState = newState;
        
        // 只在MainScene中更新UI
        if (UIManager.Instance != null)
        {
            switch (newState)
            {
                case GameState.Ready:
                    UIManager.Instance.SwitchToReadyState();
                    break;
                case GameState.Playing:
                    UIManager.Instance.SwitchToGameState();
                    break;
                case GameState.GameOver:
                    UIManager.Instance.SwitchToGameOverState();
                    break;
            }
        }
    }
    
    public float GetRemainingTime()
    {
        return remainingTime;
    }

    public void OnLevelComplete()
    {
        StartCoroutine(LevelCompleteSequence());
    }
    
    private IEnumerator LevelCompleteSequence()
    {
        DraggableObject.SetGlobalFrozen(true);
        yield return new WaitForSeconds(levelCompleteDelay);
    }

    public void OnCountdownComplete()
    {
        float finalCoverage = DoorAreaManager.Instance.GetCurrentCoverage();
        if (UIManager.Instance != null)
        {
            UIManager.Instance.StartGameEndSequence(finalCoverage, winThreshold);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            
            // 清理所有管理器的静态引用
            UIManager.ClearStaticReferences();
            DraggableObject.ClearStaticReferences();
            CollectibleObject.ClearStaticReferences();
            
            StopAllCoroutines();
        }
    }

    public void CleanupBeforeSceneChange()
    {
        // 优先清理破碎物
        FragileObject.CleanupBrokenPieces();
        
        StopAllCoroutines();
        
        // 重置游戏状态
        CurrentState = GameState.Menu;  // 返回到Menu状态
        Time.timeScale = 1f;
    }
}