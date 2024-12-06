using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIButtonActions : MonoBehaviour
{
    // Start is called before the first frame update
    public string SceneName;

    public void LoadScene()
    {
        SceneManager.LoadScene(SceneName);
    }


}
