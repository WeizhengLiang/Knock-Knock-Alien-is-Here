using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CollectionSceneManager : MonoBehaviour
{
    [System.Serializable]
    public class CollectibleDisplay
    {
        public CollectibleType type;
        public GameObject displayObject;      // 右侧展示面板
        public GameObject lockCover;          // 锁定遮罩
        public Button collectibleButton;      // 左侧可点击的收集物按钮
    }
    
    [SerializeField] private CollectibleDisplay[] collectibles;
    [SerializeField] private Button backButton;

    private void Start()
    {
        backButton?.onClick.AddListener(() => SceneController.Instance.ExitCollection());
        SetupCollectibleButtons();
        UpdateCollectibleDisplay();
    }

    private void SetupCollectibleButtons()
    {
        foreach (var item in collectibles)
        {
            // 添加鼠标事件监听
            EventTrigger trigger = item.collectibleButton.gameObject.AddComponent<EventTrigger>();
            
            // 鼠标进入事件
            EventTrigger.Entry enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) => { OnPointerEnter(item); });
            trigger.triggers.Add(enterEntry);
            
            // 鼠标离开事件
            EventTrigger.Entry exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((data) => { OnPointerExit(item); });
            trigger.triggers.Add(exitEntry);
        }
    }
    
    private void OnPointerEnter(CollectibleDisplay item)
    {
        if (CollectibleManager.Instance.IsCollectibleUnlocked(item.type))
        {
            item.displayObject?.SetActive(true);
        }
    }
    
    private void OnPointerExit(CollectibleDisplay item)
    {
        item.displayObject?.SetActive(false);
    }
    
    private void UpdateCollectibleDisplay()
    {
        foreach (var item in collectibles)
        {
            bool isUnlocked = CollectibleManager.Instance.IsCollectibleUnlocked(item.type);
            item.displayObject?.SetActive(false);  // 初始时隐藏所有详情面板
            item.lockCover?.SetActive(!isUnlocked);
            
            // 如果未解锁，禁用按钮交互
            if (item.collectibleButton != null)
            {
                item.collectibleButton.interactable = isUnlocked;
            }
        }
    }

    private void OnDestroy()
    {
        backButton?.onClick.RemoveAllListeners();
    }
}