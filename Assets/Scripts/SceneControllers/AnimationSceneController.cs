using UnityEngine;

public class AnimationSceneController : MonoBehaviour
{
    public void OnNextButtonClicked()
    {
        SceneController.Instance.ReturnToComic();
    }

    public void OnSkipButtonClicked()
    {
        SceneController.Instance.ReturnToComic();
    }
}