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
    [SerializeField] private GameObject gameOverPanel;

    [Header("Game UI Elements")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image HP;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;    // 在 Inspector 中关联重启按钮
    [SerializeField] private Button startButton;    // 在 Inspector 中关联开始按钮

    [Header("End Game UI")]
    [SerializeField] private TextMeshProUGUI coverageText;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private GameObject winUI;
    [SerializeField] private GameObject loseUI;
    [SerializeField] private Button returnButton;
    [SerializeField] private float coverageAnimSpeed = 50f;
    [SerializeField] private TextMeshProUGUI winScoreText;     // Win UI 下的分数文本
    [SerializeField] private TextMeshProUGUI loseScoreText;    // Lose UI 下的分数文本
    [SerializeField] private float endScoreDelay = 0.5f;       // 分数显示完成后的等待时间

    [Header("Countdown UI")]
    [SerializeField] private GameObject finalCountdownUI;   // 最后3秒倒计时UI
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Start Countdown UI")]
    [SerializeField] private GameObject startCountdownUI;    // 开始倒计时UI
    [SerializeField] private TextMeshProUGUI startCountdownText;

    [Header("New Collectible Icon")]
    [SerializeField] private GameObject[] newCollectibleIcons; // 所有需要显示new图标的位置

    [Header("Real Ending")]
    [SerializeField] private GameObject realEndingBG;

    [Header("Unlock Message UI")] 
    [SerializeField] private GameObject winUnlockMessagePanel;    // Win UI 下的消息面板
    [SerializeField] private GameObject loseUnlockMessagePanel;   // Lose UI 下的消息面板
    [SerializeField] private TextMeshProUGUI[] winAlienMessages;  // Win UI 下的消息
    [SerializeField] private TextMeshProUGUI[] loseAlienMessages; // Lose UI 下的消息

    private float displayedCoverage;
    private float targetCoverage;
    private bool isAnimatingCoverage;
    private Coroutine startCountdownCoroutine;

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
        mainPanel?.SetActive(true);
        gamePanel?.SetActive(false);
        gameOverPanel?.SetActive(false);
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

        float fillAmount = Mathf.Clamp01(time / 20f);
        HP.fillAmount = fillAmount;
    }

    private void SetupButtons()
    {
        restartButton.onClick.AddListener(OnRestartButtonClicked);
        startButton.onClick.AddListener(OnStartButtonClicked);
        returnButton.onClick.AddListener(() => SceneController.Instance.ReturnToMainMenu());

        // 初始时隐藏所有结算按钮
        returnButton.gameObject.SetActive(false);
    }

    public void ShowMainPanel(bool show)
    {
        mainPanel.SetActive(show);
    }

    public void ShowGamePanel(bool show)
    {
        gamePanel.SetActive(show);
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

    // 换到游戏状态时调用此方法
    public void SwitchToReadyState()
    {
        ShowMainPanel(true);
        ShowGamePanel(false);
        ShowGameOverPanel(false);
    }

    public void SwitchToGameState()
    {
        ShowMainPanel(false);
        ShowGamePanel(true);
        ShowGameOverPanel(false);

        // 初始显示普通计时器
        finalCountdownUI.SetActive(false);
    }

    // 切换到游戏结束状态时调用此方法
    public void SwitchToGameOverState()
    {
        if (CanShowRealEnding())
        {
            // 如果满足条件，显示真结局
            StartCoroutine(ShowRealEndingSequence());
        }
        else
        {
            // 常规显示游戏结束面板
            ShowMainPanel(false);
            ShowGamePanel(false);
            ShowGameOverPanel(true);
            UpdateNewCollectibleIcons();
        }
    }

    private bool CanShowRealEnding()
    {
        // 检查是否所有收集品都已解锁
        bool allCollectiblesUnlocked = CollectibleManager.Instance != null && 
                                     CollectibleManager.Instance.UnlockedCollectibles.All(pair => pair.Value);

        // 检查是否只用标本挡住了门
        bool onlySpecimenUsed = DoorAreaManager.Instance != null && 
                               DoorAreaManager.Instance.IsOnlySpecimenUsedForCoverage();

        return allCollectiblesUnlocked && onlySpecimenUsed;
    }

    private IEnumerator ShowRealEndingSequence()
    {
        // 隐藏所有其他面板
        ShowMainPanel(false);
        ShowGamePanel(false);
        ShowGameOverPanel(false);

        // 显示真结局背景
        if (realEndingBG != null)
        {
            realEndingBG.SetActive(true);
        }

        // 开始异步加载场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("RealEndingScene");
        asyncLoad.allowSceneActivation = false;  // 暂时不激活新场景

        // 等待1秒
        yield return new WaitForSeconds(1f);

        // 允许激活新场景
        asyncLoad.allowSceneActivation = true;
    }

    private void OnRestartButtonClicked()
    {
        GameManager.Instance.RestartGame();
    }

    private void OnStartButtonClicked()
    {
        GameManager.Instance.StartGame();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();

        restartButton.onClick.RemoveListener(OnRestartButtonClicked);
        startButton.onClick.RemoveListener(OnStartButtonClicked);
        returnButton.onClick.RemoveAllListeners();

        if (Instance == this)
        {
            Instance = null;
        }

        // 清理UI面板引用
        mainPanel = null;
        gamePanel = null;
        gameOverPanel = null;
    }

    public static void ClearStaticReferences()
    {
        Instance = null;
    }

    public void StartGameEndSequence(float finalCoverage, float winThreshold)
    {
        if (!isAnimatingCoverage)
        {
            isAnimatingCoverage = true;
            targetCoverage = finalCoverage;
            displayedCoverage = 0f;
            StartCoroutine(AnimateCoverageSequence(winThreshold));
        }
    }

    private IEnumerator AnimateCoverageSequence(float winThreshold)
    {
        // 等待无效物体消失动画完成
        yield return StartCoroutine(HandleInvalidObjects());

        // 开始播放分数攀升音效
        SoundManager.Instance.PlaySoundFromResources("Sound/Reveal", "Reveal", true, 1.0f);

        // 动画显示百分比
        while (isAnimatingCoverage)
        {
            displayedCoverage = Mathf.MoveTowards(displayedCoverage, targetCoverage, coverageAnimSpeed * Time.deltaTime);
            UpdateCoverageDisplay(displayedCoverage);

            // 检查是否达到胜利阈值
            if (targetCoverage >= winThreshold && displayedCoverage >= winThreshold)
            {
                isAnimatingCoverage = false;
                // 停止分数攀升音效
                SoundManager.Instance.StopSound("Reveal");
                
                // 更新胜利UI的分数显示，向下取整
                if (winScoreText != null)
                {
                    winScoreText.text = $"{Mathf.FloorToInt(displayedCoverage)}%";
                }
                
                // 等待指定时间
                yield return new WaitForSeconds(endScoreDelay);
                yield return StartCoroutine(ShowWinSequence());
                break;
            }

            // 检查是否达到最终分数
            if (Mathf.Approximately(displayedCoverage, targetCoverage))
            {
                isAnimatingCoverage = false;
                // 停止分数攀升音效
                SoundManager.Instance.StopSound("Reveal");
                
                // 更新失败UI的分数显示，向下取整
                if (loseScoreText != null)
                {
                    loseScoreText.text = $"{Mathf.FloorToInt(displayedCoverage)}%";
                }
                
                if (targetCoverage < winThreshold)
                {
                    // 等待指定时间
                    yield return new WaitForSeconds(endScoreDelay);
                    yield return StartCoroutine(ShowLoseSequence());
                }
                break;
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
        ShowEndGameContent(true);
        yield return null;
    }

    private IEnumerator ShowLoseSequence()
    {
        ShowEndGameContent(false);
        yield return null;
    }

    private void ShowEndGameContent(bool isWin)
    {
        // 先隐藏其他面板
        mainPanel?.SetActive(false);
        gamePanel?.SetActive(false);
        gameOverPanel.SetActive(true);
        
        // 重置按钮状态
        returnButton.gameObject.SetActive(false);

        // 设置胜负UI
        winUI.SetActive(isWin);
        loseUI.SetActive(!isWin);
        
        // 停止计算音效
        SoundManager.Instance.StopSound("Computing");

        // 播放音效
        if (isWin)
        {
            SoundManager.Instance.PlaySoundFromResources("Sound/Victory", "Victory", false, 1.0f);
        }
        else
        {
            SoundManager.Instance.PlaySoundFromResources("Sound/Defeat", "Defeat", false, 1.0f);
        }

        // 显示按钮
        returnButton.gameObject.SetActive(true);

        // 检查并显示收藏品解锁信息
        if (CollectibleManager.Instance != null && CollectibleManager.Instance.HasNewUnlocksThisSession())
        {
            StartCoroutine(ShowUnlockMessageSequence(isWin));
        }
        
        // 更新收藏品图标
        UpdateNewCollectibleIcons();
    }

    private void UpdateCoverageDisplay(float coverage)
    {
        if (coverageText != null)
        {
            coverageText.text = $"{coverage:F1}%";  // 游戏中的实时显示保持一位小数
        }
    }

    public void UpdateNewCollectibleIcons()
    {
        if (CollectibleManager.Instance != null)
        {
            // 检查是否有未查看的收集物
            bool hasUnviewedCollectibles = CollectibleManager.Instance.HasUnviewedCollectibles();
            foreach (var icon in newCollectibleIcons)
            {
                if (icon != null)
                {
                    icon.SetActive(hasUnviewedCollectibles);
                }
            }
        }
    }

    public void ShowStartCountdown()
    {
        if (startCountdownCoroutine != null)
        {
            StopCoroutine(startCountdownCoroutine);
        }
        startCountdownCoroutine = StartCoroutine(StartCountdownSequence());
    }

    private IEnumerator StartCountdownSequence()
    {
        if (startCountdownUI != null)
        {
            startCountdownUI.SetActive(true);
            
            for (int i = 3; i > 0; i--)
            {
                if (startCountdownText != null)
                {
                    startCountdownText.text = i.ToString();
                }
                yield return new WaitForSeconds(1f);
            }
            
            startCountdownUI.SetActive(false);
        }
    }

    public void SwitchToGamePanel()
    {
        mainPanel?.SetActive(false);
        gamePanel?.SetActive(true);
    }
    
    public IEnumerator ShowStartCountdownAndWait()
    {
        if (startCountdownUI != null)
        {
            startCountdownUI.SetActive(true);
            
            // 3-2-1倒计时
            for (int i = 3; i > 0; i--)
            {
                if (startCountdownText != null)
                {
                    startCountdownText.text = i.ToString();
                }
                yield return new WaitForSeconds(1f);
            }
            
            startCountdownUI.SetActive(false);
        }
    }

    // 在场景加载完成时调用
    public void OnMainSceneLoaded()
    {
        UpdateNewCollectibleIcons();
    }

    private IEnumerator ShowUnlockMessageSequence(bool isWin)
    {
        // 先等待分数动画完全显示
        while (isAnimatingCoverage)
        {
            yield return null;
        }

        GameObject messagePanel = isWin ? winUnlockMessagePanel : loseUnlockMessagePanel;
        TextMeshProUGUI[] messages = isWin ? winAlienMessages : loseAlienMessages;

        if (messagePanel == null || messages == null) 
        {
            Debug.LogWarning($"Message panel or messages array is null for {(isWin ? "win" : "lose")} UI");
            yield break;
        }

        // 确保所有消息初始状态为隐藏
        foreach (var message in messages)
        {
            if (message != null)
            {
                message.gameObject.SetActive(false);
            }
        }

        // 确保另一个面板是隐藏的
        if (isWin)
        {
            loseUnlockMessagePanel?.SetActive(false);
        }
        else
        {
            winUnlockMessagePanel?.SetActive(false);
        }

        var newlyUnlocked = CollectibleManager.Instance.GetNewlyUnlockedCollectibles();
        if (newlyUnlocked.Count > 0)
        {
            messagePanel.SetActive(true);
            yield return new WaitForSeconds(0.5f);  // 给玩家一点时间注意到面板的出现

            // 如果有多个解锁，随机选择一个显示
            CollectibleType typeToShow = newlyUnlocked.Count > 1 
                ? newlyUnlocked.ElementAt(Random.Range(0, newlyUnlocked.Count))
                : newlyUnlocked.First();

            // 显示对应消息
            int messageIndex = (int)typeToShow;
            if (messageIndex < messages.Length && messages[messageIndex] != null)
            {
                messages[messageIndex].gameObject.SetActive(true);
                yield return StartCoroutine(TypeText(messages[messageIndex], 0.03f));  // 调整打字速度
            }
            else
            {
                Debug.LogWarning($"No message found for collectible type: {typeToShow}");
            }
        }
    }

    private IEnumerator TypeText(TextMeshProUGUI textComponent, float delay)
    {
        string fullText = textComponent.text;
        textComponent.text = "";
        foreach (char letter in fullText.ToCharArray())
        {
            textComponent.text += letter;
            yield return new WaitForSeconds(delay);
        }
    }
}