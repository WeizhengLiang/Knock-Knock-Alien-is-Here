using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    [Header("UI Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject gamePanel;   
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    
    [Header("Game UI Elements")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;    // 在 Inspector 中关联重启按钮
    [SerializeField] private Button startButton;    // 在 Inspector 中关联开始按钮
    [SerializeField] private Button pauseButton;    // 在 Inspector 中关联暂停按钮
    [SerializeField] private Button resumeButton;    // 在 Inspector 中关联继续按钮

    [Header("End Game UI")]
    [SerializeField] private TextMeshProUGUI coverageText;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private GameObject winUI;
    [SerializeField] private GameObject loseUI;
    [SerializeField] private Button returnButton;
    [SerializeField] private Button futureDayButton;    // 胜利时显示
    [SerializeField] private Button timeMachineButton;  // 失败时显示
    [SerializeField] private Button collectionButton;
    [SerializeField] private float coverageAnimSpeed = 50f;

    [Header("Countdown UI")]
    [SerializeField] private GameObject finalCountdownUI;   // 最后3秒倒计时UI
    [SerializeField] private TextMeshProUGUI countdownText;

    private float displayedCoverage;
    private float targetCoverage;
    private bool isAnimatingCoverage;
    
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

        SetupButtons();
    }
    
    private void Start()
    {
        // 初始化时隐藏所有面板
        mainPanel.SetActive(true);
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
        if (time <= 3f)
        {
            // 显示大数字倒计时
            countdownText.text = Mathf.CeilToInt(time).ToString();
        }
        else
        {
            // 显示普通计时器
            timerText.text = $"Time: {time:F1}";
        }
    }

    private void SetupButtons()
    {
        restartButton.onClick.AddListener(OnRestartButtonClicked);
        startButton.onClick.AddListener(OnStartButtonClicked);
        pauseButton.onClick.AddListener(OnPauseButtonClicked);
        resumeButton.onClick.AddListener(OnResumeButtonClicked);
        // Return按钮
        returnButton.onClick.AddListener(() => SceneController.Instance.ReturnToMainMenu());
        
        // 胜利场景按钮
        futureDayButton.onClick.AddListener(() => SceneController.Instance.ShowWinAnimation());
        
        // 失败场景按钮
        timeMachineButton.onClick.AddListener(() => SceneController.Instance.ShowLoseAnimation());
        
        // Collection按钮
        collectionButton.onClick.AddListener(() => Debug.Log("open Collection"));
        
        // 初始时隐藏所有结算按钮
        returnButton.gameObject.SetActive(false);
        futureDayButton.gameObject.SetActive(false);
        timeMachineButton.gameObject.SetActive(false);
        collectionButton.gameObject.SetActive(false);
    }
    
    public void ShowMainPanel(bool show)
    {
        mainPanel.SetActive(show);
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

    public void ShowFinalCountdown()
    {
        // 切换到最后3秒的倒计时显示
        finalCountdownUI.SetActive(true);
        
        // 可以在这里添加倒计时动画效果
        // 例如：缩放动画、颜色变化等
    }
    
    // 切换到游戏状态时调用此方法
    public void SwitchToMainState()
    {
        ShowMainPanel(true);
        ShowGamePanel(false);
        ShowPausePanel(false);
        ShowGameOverPanel(false);
    }
    
    public void SwitchToGameState()
    {
        ShowMainPanel(false);
        ShowGamePanel(true);
        ShowPausePanel(false);
        ShowGameOverPanel(false);

        // 初始显示普通计时器
        finalCountdownUI.SetActive(false);
    }
    
    // 切换到暂停状态时调用此方法
    public void SwitchToPauseState()
    {
        ShowMainPanel(false);
        ShowGamePanel(false);
        ShowPausePanel(true);
        ShowGameOverPanel(false);
    }
    
    // 切换到游戏结束状态时调用此方法
    public void SwitchToGameOverState()
    {
        ShowMainPanel(false);
        ShowGamePanel(false);
        ShowPausePanel(false);
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
        restartButton.onClick.RemoveListener(OnRestartButtonClicked);
        startButton.onClick.RemoveListener(OnStartButtonClicked);
        pauseButton.onClick.RemoveListener(OnPauseButtonClicked);
        resumeButton.onClick.RemoveListener(OnResumeButtonClicked);
        returnButton.onClick.RemoveAllListeners();
        futureDayButton.onClick.RemoveAllListeners();
        timeMachineButton.onClick.RemoveAllListeners();
        collectionButton.onClick.RemoveAllListeners();
    }

    public void StartGameEndSequence(float finalCoverage, float winThreshold)
    {
        targetCoverage = finalCoverage;
        displayedCoverage = 0f;
        isAnimatingCoverage = true;
        StartCoroutine(AnimateCoverageSequence(winThreshold));
    }
    
    private IEnumerator AnimateCoverageSequence(float winThreshold)
    {
        // 等待无效物体消失动画完成
        yield return StartCoroutine(HandleInvalidObjects());
        
        // 动画显示百分比
        while (isAnimatingCoverage)
        {
            displayedCoverage = Mathf.MoveTowards(displayedCoverage, targetCoverage, coverageAnimSpeed * Time.deltaTime);
            UpdateCoverageDisplay(displayedCoverage);
            
            // 检查是否达到胜利阈值
            if (targetCoverage >= winThreshold && displayedCoverage >= winThreshold)
            {
                yield return StartCoroutine(ShowWinSequence());
            }
            
            // 检查是否达到最终分数
            if (Mathf.Approximately(displayedCoverage, targetCoverage))
            {
                isAnimatingCoverage = false;
                if (targetCoverage < winThreshold)
                {
                    yield return StartCoroutine(ShowLoseSequence());
                }
            }
            
            yield return null;
        }
    }

    private IEnumerator HandleInvalidObjects()
    {
        var invalidObjects = DraggableObject.AllDraggableObjects
            .Where(obj => obj.IsInInvalidPosition() || obj.IsDragging());
            
        foreach (var obj in invalidObjects)
        {
            // 触发消失动画
            obj.TriggerDisappearAnimation();
        }
        
        yield return new WaitForSeconds(1f);
    }
    
    // private IEnumerator BlinkAndDestroyObject(DraggableObject obj)
    // {
    //     SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
    //     float elapsedTime = 0f;
        
    //     while (elapsedTime < 1f)
    //     {
    //         renderer.enabled = !renderer.enabled;
    //         yield return new WaitForSeconds(0.1f);
    //         elapsedTime += 0.1f;
    //     }
        
    //     Destroy(obj.gameObject);
    // }
    
    private IEnumerator ShowWinSequence()
    {
        winUI.SetActive(true);
        resultText.text = "Victory!";
        // 显示胜利相关按钮
        returnButton.gameObject.SetActive(true);
        futureDayButton.gameObject.SetActive(true);
        collectionButton.gameObject.SetActive(true);
        yield return null;
    }
    
    private IEnumerator ShowLoseSequence()
    {
        loseUI.SetActive(true);
        resultText.text = "Lose...";
        // 显示失败相关按钮
        returnButton.gameObject.SetActive(true);
        timeMachineButton.gameObject.SetActive(true);
        collectionButton.gameObject.SetActive(true);
        yield return null;
    }
    
    private void UpdateCoverageDisplay(float coverage)
    {
        if (coverageText != null)
        {
            coverageText.text = $"{coverage:F1}%";
        }
    }
}