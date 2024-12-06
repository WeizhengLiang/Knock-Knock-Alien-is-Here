using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ComicNextPage : MonoBehaviour
{

    public List<GameObject> comicsList;
    public GameObject skipButton, nextButton, gameStartButton;
    public float skipTimeStep = .5f;
    private int currentPageNum = 0;
    void Start()
    {
        foreach (GameObject comic in comicsList)
        {
            comic.SetActive(false);
        }
        comicsList[0].SetActive(true);
        gameStartButton.SetActive(false);
    }

    public void StartGame()
    {
        SceneController.Instance.StartGameFromComic();
    }

    public void NextPage()
    {
        currentPageNum++;

        comicsList[currentPageNum].SetActive(true);
        if (currentPageNum == comicsList.Count - 1)
        {
            skipButton.SetActive(false);
            nextButton.SetActive(false);
            gameStartButton.SetActive(true);
        }

    }

    public void Skip()
    {
        skipButton.SetActive(false);
        nextButton.SetActive(false);
        StartCoroutine(ActivateComicsGradually());
        gameStartButton.SetActive(true);
    }

    private IEnumerator ActivateComicsGradually()
    {
        for (int i = currentPageNum + 1; i < comicsList.Count; i++)
        {
            comicsList[i].SetActive(true);
            yield return new WaitForSeconds(skipTimeStep);
        }
    }

}
