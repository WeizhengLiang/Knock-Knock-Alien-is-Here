using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections.Generic;

public class UnlockMessageUI : MonoBehaviour
{
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TextMeshProUGUI[] alienMessages; // 六个TMP对应六个收集物
    
    private void Start()
    {
        // 初始时隐藏面板
        messagePanel?.SetActive(false);
        HideAllMessages();
    }
    
    public void ShowUnlockMessage()
    {
        // 获取本局解锁的收集物
        var newlyUnlocked = CollectibleManager.Instance.GetNewlyUnlockedCollectibles();
        
        if (newlyUnlocked.Count == 0)
        {
            messagePanel?.SetActive(false);
            return;
        }
        
        messagePanel?.SetActive(true);
        
        // 如果有多个解锁，随机选择一个显示
        int randomIndex = -1;
        if (newlyUnlocked.Count > 1)
        {
            randomIndex = Random.Range(0, newlyUnlocked.Count);
        }
        
        // 显示对应消息
        for (int i = 0; i < alienMessages.Length; i++)
        {
            if (alienMessages[i] != null)
            {
                bool shouldShow = newlyUnlocked.Count == 1 ? 
                    newlyUnlocked.Contains((CollectibleType)i) :
                    newlyUnlocked.ElementAt(randomIndex) == (CollectibleType)i;
                    
                alienMessages[i].gameObject.SetActive(shouldShow);
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