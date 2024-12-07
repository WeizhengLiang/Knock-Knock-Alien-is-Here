using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndSceneUI : MonoBehaviour
{
    [Header("New Collectible Icon")]
    [SerializeField] private GameObject newCollectibleIcon;
    
    [Header("Unlock Message")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TextMeshProUGUI[] alienMessages;

    private void Start()
    {
        // 初始化UI状态
        newCollectibleIcon?.SetActive(false);
        
        // 如果在真结局场景，只处理New图标
        if (SceneManager.GetActiveScene().name == "RealEndingScene")
        {
            UpdateNewIcon();
            return;
        }
        
        // 普通场景的其他初始化
        messagePanel?.SetActive(false);
        HideAllMessages();
        
        if (CollectibleManager.Instance != null)
        {
            bool hasNewUnlocksThisSession = CollectibleManager.Instance.HasNewUnlocksThisSession();
            if (hasNewUnlocksThisSession && messagePanel != null)
            {
                ShowUnlockMessage();
            }
            
            UpdateNewIcon();
        }
    }

    private void UpdateNewIcon()
    {
        if (newCollectibleIcon != null && CollectibleManager.Instance != null)
        {
            newCollectibleIcon.SetActive(CollectibleManager.Instance.HasUnviewedCollectibles());
        }
    }

    private void ShowUnlockMessage()
    {
        var newlyUnlocked = CollectibleManager.Instance.GetNewlyUnlockedCollectibles();
        
        if (newlyUnlocked.Count == 0)
        {
            messagePanel?.SetActive(false);
            return;
        }
        
        messagePanel?.SetActive(true);
        
        // 如果有多个解锁，随机选择一个显示
        CollectibleType typeToShow;
        if (newlyUnlocked.Count > 1)
        {
            int randomIndex = Random.Range(0, newlyUnlocked.Count);
            typeToShow = newlyUnlocked.ElementAt(randomIndex);
        }
        else
        {
            typeToShow = newlyUnlocked.First();
        }
        
        // 显示对应消息
        for (int i = 0; i < alienMessages.Length; i++)
        {
            if (alienMessages[i] != null)
            {
                alienMessages[i].gameObject.SetActive((CollectibleType)i == typeToShow);
            }
        }
    }
    
    private void HideAllMessages()
    {
        foreach (var message in alienMessages)
        {
            if (message != null)
            {
                message.gameObject.SetActive(false);
            }
        }
    }
}
