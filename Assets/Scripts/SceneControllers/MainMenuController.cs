using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public void OnStartButtonClicked()
    {
        SceneController.Instance.StartGameFromMenu();
    }
}