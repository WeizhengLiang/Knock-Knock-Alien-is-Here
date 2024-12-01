using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour
{
    [Header("Main Menu UI")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button collectionButton;
    [SerializeField] private GameObject newCollectibleIcon;

    private void Start()
    {
        SetupButtons();
        UpdateNewCollectibleIcon();
    }

    private void SetupButtons()
    {
        startButton?.onClick.AddListener(() => SceneController.Instance.StartGameFromMenu());
        collectionButton?.onClick.AddListener(() => SceneController.Instance.EnterCollection());
    }

    private void UpdateNewCollectibleIcon()
    {
        if (newCollectibleIcon != null)
        {
            newCollectibleIcon.SetActive(CollectibleManager.Instance.HasNewCollectible());
        }
    }
}