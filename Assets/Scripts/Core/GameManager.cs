using System.Collections;
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
    
    // 游戏状态变量
    public GameState CurrentState { get; private set; }
    private float remainingTime;

    [Header("游戏设置")]
    [SerializeField] private float levelCompleteDelay = 1f;    // 完成后的延迟时间    
    [SerializeField] private float totalGameTime = 20f;    // 总游戏时间
    [SerializeField] private float finalCountdownTime = 3f; // 最后的倒计时时间
    [SerializeField] private float winThreshold = 70f;
    private bool isFinalCountdown = false;
    
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
        // 检查是否进入最后3秒
        if (!isFinalCountdown && remainingTime <= finalCountdownTime)
        {
            isFinalCountdown = true;
            UIManager.Instance.ShowFinalCountdown();
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
        remainingTime = 0;
        SetGameState(GameState.GameOver);
        OnCountdownComplete();
    }

    public void RestartGame()
    {
        // 重置游戏时间
        remainingTime = totalGameTime;
        
        // 重置时间缩放（以防游戏在暂停状态重启）
        Time.timeScale = 1;

        DraggableObject.ResetGlobalState();
        
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
            case GameState.Menu:
                UIManager.Instance.SwitchToMainState();
                break;
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

    public void OnLevelComplete()
    {
        StartCoroutine(LevelCompleteSequence());
    }
    
    private IEnumerator LevelCompleteSequence()
    {
        // 禁用所有物体的拖拽
        DraggableObject.SetGlobalFrozen(true);
        
        // 等待一段时间
        yield return new WaitForSeconds(levelCompleteDelay);

        // 可以在这里添加其他胜利效果
        // 例如：播放音效、粒子效果等
    }
    public void OnCountdownComplete()
    {
        float finalCoverage = DoorAreaManager.Instance.GetCurrentCoverage();
        UIManager.Instance.StartGameEndSequence(finalCoverage, winThreshold);
    }
}
