using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    private static AnimationManager instance;
    public static AnimationManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AnimationManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("AnimationManager");
                    instance = go.AddComponent<AnimationManager>();
                }
            }
            return instance;
        }
    }

    public void PlayFlickering(Animator animator)
    {
        if (animator != null)
        {
            animator.Play("Flickering");
        }
    }
}