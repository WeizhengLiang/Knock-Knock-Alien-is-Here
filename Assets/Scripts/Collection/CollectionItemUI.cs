using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CollectionItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Item Visuals")]
    [SerializeField] private Image itemImage;
    [SerializeField] private Material unlockedMaterial;
    [SerializeField] private Material lockedMaterial;
    
    [Header("Item Data")]
    private CollectibleData itemData;
    private bool isUnlocked;
    
    // 引用CollectionPanel来更新右侧信息
    private CollectionPanel parentPanel;

    public void Setup(CollectibleData data, bool unlocked, CollectionPanel panel)
    {
        itemData = data;
        isUnlocked = unlocked;
        parentPanel = panel;
        itemImage.sprite = data.icon;
        
        // 设置显示状态
        itemImage.material = isUnlocked ? unlockedMaterial : lockedMaterial;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isUnlocked)
        {
            parentPanel.ShowItemDetail(itemData);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (isUnlocked)
        {
            parentPanel.HideItemDetail();
        }
    }
}