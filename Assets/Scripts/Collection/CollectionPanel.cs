using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CollectionPanel : MonoBehaviour
{
    [Header("Panel Layout")]
    [SerializeField] private GameObject leftPanel;      // 书架背景
    [SerializeField] private GameObject rightPanel;     // 详情面板
    [SerializeField] private Button closeButton;        // 关闭按钮
    
    [Header("Detail Display")]
    [SerializeField] private Image comicDisplay;        // 上方漫画显示
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI alienResponseText;
    
    [Header("Collection Grid")]
    [SerializeField] private Transform[] itemPositions; // 书架上的固定位置
    [SerializeField] private GameObject collectionItemPrefab;
    
    private void Start()
    {
        rightPanel.SetActive(false);
        closeButton.onClick.AddListener(() => {
            UIManager.Instance.CloseCollectionPanel();
        });
    }
    
    public void ShowItemDetail(CollectibleData data)
    {
        rightPanel.SetActive(true);
        
        // 更新详情显示
        itemNameText.text = data.itemName;
        descriptionText.text = data.description;
        alienResponseText.text = data.alienResponse;
        
        if (data.comicSprite != null)
        {
            comicDisplay.sprite = data.comicSprite;
            comicDisplay.gameObject.SetActive(true);
        }
        else
        {
            comicDisplay.gameObject.SetActive(false);
        }
    }
    
    public void HideItemDetail()
    {
        rightPanel.SetActive(false);
    }
    
    public void SetupCollectionItems(CollectibleData[] items, Dictionary<CollectibleType, bool> unlockedStates)
    {
        // 清除现有项
        foreach (Transform pos in itemPositions)
        {
            foreach (Transform child in pos)
            {
                Destroy(child.gameObject);
            }
        }
        
        // 创建新项
        for (int i = 0; i < items.Length && i < itemPositions.Length; i++)
        {
            GameObject itemObj = Instantiate(collectionItemPrefab, itemPositions[i]);
            CollectionItemUI itemUI = itemObj.GetComponent<CollectionItemUI>();
            itemUI.Setup(items[i], unlockedStates[items[i].type], this);
        }
    }
    
    private void OnDestroy()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
        }
    }
}