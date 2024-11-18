using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    [Header("UI Panels")]
    [SerializeField] private GameObject gamePanel;    // 新增游戏面板
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    
    [Header("Game UI Elements")]
    [SerializeField] private TextMeshProUGUI timerText;
    // 可以在这里添加其他游戏中需要的 UI 元素

    [Header("Buttons")]
    [SerializeField] private Button restartButton;    // 在 Inspector 中关联重启按钮
    [SerializeField] private Button startButton;    // 在 Inspector 中关联开始按钮
    [SerializeField] private Button pauseButton;    // 在 Inspector 中关联暂停按钮
    [SerializeField] private Button resumeButton;    // 在 Inspector 中关联继续按钮
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }

        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(OnPauseButtonClicked);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(OnResumeButtonClicked);
        }
    }
    
    private void Start()
    {
        // 初始化时隐藏所有面板
        gamePanel.SetActive(false);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }
    
    private void Update()
    {
        if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            UpdateTimerDisplay();
        }
    }
    
    private void UpdateTimerDisplay()
    {
        float time = GameManager.Instance.GetRemainingTime();
        timerText.text = $"Time: {time:F1}";
    }
    
    public void ShowGamePanel(bool show)
    {
        gamePanel.SetActive(show);
    }
    
    public void ShowPausePanel(bool show)
    {
        pausePanel.SetActive(show);
    }
    
    public void ShowGameOverPanel(bool show)
    {
        gameOverPanel.SetActive(show);
    }
    
    // 切换到游戏状态时调用此方法
    public void SwitchToGameState()
    {
        ShowGamePanel(true);
        ShowPausePanel(false);
        ShowGameOverPanel(false);
    }
    
    // 切换到暂停状态时调用此方法
    public void SwitchToPauseState()
    {
        ShowPausePanel(true);
    }
    
    // 切换到游戏结束状态时调用此方法
    public void SwitchToGameOverState()
    {
        ShowGamePanel(false);
        ShowGameOverPanel(true);
    }

    private void OnRestartButtonClicked()
    {
        GameManager.Instance.RestartGame();
    }

    private void OnStartButtonClicked()
    {
        GameManager.Instance.StartGame();
    }

    private void OnPauseButtonClicked()
    {
        GameManager.Instance.PauseGame();
    }

    private void OnResumeButtonClicked()
    {
        GameManager.Instance.ResumeGame();
    }

    private void OnDestroy()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(OnRestartButtonClicked);
        }

        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartButtonClicked);
        }   

        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(OnPauseButtonClicked);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(OnResumeButtonClicked);
        }
    }
}