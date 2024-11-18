using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    // 游戏状态枚举
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver
    }
    
    // 游戏配置
    private const float GAME_DURATION = 20f;
    
    // 游戏状态变量
    public GameState CurrentState { get; private set; }
    private float remainingTime;
    
    private void Awake()
    {
        // 单例模式实现
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
        if (remainingTime <= 0)
        {
            EndGame();
        }
    }
    
    public void StartGame()
    {
        remainingTime = GAME_DURATION;
        SetGameState(GameState.Playing);
    }
    
    public void PauseGame()
    {
        SetGameState(GameState.Paused);
        Time.timeScale = 0;
    }
    
    public void ResumeGame()
    {
        SetGameState(GameState.Playing);
        Time.timeScale = 1;
    }
    
    public void EndGame()
    {
        SetGameState(GameState.GameOver);
        // TODO: 实现游戏结束逻辑
    }

    public void RestartGame()
    {
        // 重置游戏时间
        remainingTime = GAME_DURATION;
        
        // 重置时间缩放（以防游戏在暂停状态重启）
        Time.timeScale = 1;
        
        // 重新加载当前场景
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
        
        // 设置游戏状态为开始
        SetGameState(GameState.Playing);
    }
    
    private void SetGameState(GameState newState)
    {
        CurrentState = newState;
        
        // 更新UI显示
        switch (newState)
        {
            case GameState.Playing:
                UIManager.Instance.SwitchToGameState();
                break;
            case GameState.Paused:
                UIManager.Instance.SwitchToPauseState();
                break;
            case GameState.GameOver:
                UIManager.Instance.SwitchToGameOverState();
                break;
        }
    }
    
    public float GetRemainingTime()
    {
        return remainingTime;
    }
}