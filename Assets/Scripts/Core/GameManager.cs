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
    private bool isFinalCountdown = false;
    
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
        SetGameState(GameState.Menu);
    }
    
    // 当加载到MainScene时调用
    public void EnterMainScene()
    {
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
        remainingTime = totalGameTime;
        isFinalCountdown = false;
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
        remainingTime = totalGameTime;
        Time.timeScale = 1;
        DraggableObject.ResetGlobalState();
        
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
        
        SetGameState(GameState.Ready);  // 重启后回到Ready状态
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