using UnityEngine;

public class ComicController : MonoBehaviour
{
    public void OnNextButtonClicked()
    {
        SceneController.Instance.StartGameFromComic();
    }

    public void OnSkipButtonClicked()
    {
        SceneController.Instance.StartGameFromComic();
    }
}