using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour
{
    [Header("Main Menu UI")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button collectionButton;
    [SerializeField] private CollectionPanel collectionPanel;

    private void Start()
    {
        SetupButtons();
        collectionPanel?.gameObject.SetActive(false);
    }

    private void SetupButtons()
    {
        startButton?.onClick.AddListener(() => SceneController.Instance.StartGameFromMenu());
        collectionButton?.onClick.AddListener(OpenCollectionPanel);
    }

    public void OpenCollectionPanel()
    {
        if (CollectibleManager.Instance != null && collectionPanel != null)
        {
            collectionPanel.gameObject.SetActive(true);
            collectionPanel.SetupCollectionItems(
                CollectibleManager.Instance.CollectibleDatabase,
                CollectibleManager.Instance.UnlockedCollectibles
            );
        }
    }

    public void CloseCollectionPanel()
    {
        if (collectionPanel != null)
        {
            collectionPanel.gameObject.SetActive(false);
        }
    }
}