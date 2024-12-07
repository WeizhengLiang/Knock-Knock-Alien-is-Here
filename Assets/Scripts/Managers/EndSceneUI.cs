using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndSceneUI : MonoBehaviour
{
    [Header("New Collectible Icon")]
    [SerializeField] private GameObject newCollectibleIcon;
    
    [Header("Unlock Message")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TextMeshProUGUI[] alienMessages; // 六个TMP对应六个收集物

    private void Start()
    {
        // 初始化UI状态
        messagePanel?.SetActive(false);
        newCollectibleIcon?.SetActive(false);
        HideAllMessages();
        
        if (CollectibleManager.Instance != null)
        {
            // 只在本局有新解锁时显示消息面板
            bool hasNewUnlocksThisSession = CollectibleManager.Instance.HasNewUnlocksThisSession();
            if (hasNewUnlocksThisSession)
            {
                ShowUnlockMessage();
            }
            
            // New图标根据是否有未查看的收集物来显示
            UpdateNewIcon();
        }
    }

    private void UpdateNewIcon()
    {
        if (newCollectibleIcon != null && CollectibleManager.Instance != null)
        {
            // 使用 HasUnviewedCollectibles 来检查是否有未查看的收集物
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
