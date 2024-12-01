using UnityEngine;
using UnityEngine.UI;

public class CollectionSceneManager : MonoBehaviour
{
    [System.Serializable]
    public class CollectibleDisplay
    {
        public CollectibleType type;
        public GameObject displayObject;  // 右侧展示面板
        public GameObject lockCover;      // 锁定遮罩
    }
    
    [SerializeField] private CollectibleDisplay[] collectibles;
    [SerializeField] private Button backButton;
    
    private void Start()
    {
        backButton?.onClick.AddListener(() => SceneController.Instance.ExitCollection());
        UpdateCollectibleDisplay();
    }
    
    private void UpdateCollectibleDisplay()
    {
        foreach (var item in collectibles)
        {
            bool isUnlocked = CollectibleManager.Instance.IsCollectibleUnlocked(item.type);
            item.displayObject?.SetActive(isUnlocked);
            item.lockCover?.SetActive(!isUnlocked);
        }
    }
}