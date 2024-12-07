using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIButtonActions : MonoBehaviour
{
    // Start is called before the first frame update
    public string SceneName;
    public GameObject PageToClose, PageToOpen;


    public void LoadScene()
    {
        if (SceneName != null)
        {
            SceneManager.LoadScene(SceneName);
        }
    }

    public void ClosePage()
    {
        if (PageToClose)
        {
            PageToClose.SetActive(false);
        }
    }

    public void OpenPage()
    {
        if (PageToOpen)
        {
            PageToOpen.SetActive(true);
        }
    }


}
