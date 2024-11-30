using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComicNextPage : MonoBehaviour
{

    public List<GameObject> comicsList;
    public Button skipButton;
    private int currentPageNum = 0;

    public void NextPage()
    {
        currentPageNum++;
        comicsList[currentPageNum].SetActive(true);
        if (currentPageNum == comicsList.Count - 1)
        {
            skipButton.enabled = false;
        }
    }

    public void Skip()
    {
        for (int i = currentPageNum + 1; i < comicsList.Count; i++)
        {
            comicsList[i].SetActive(true);
        }
    }

}
