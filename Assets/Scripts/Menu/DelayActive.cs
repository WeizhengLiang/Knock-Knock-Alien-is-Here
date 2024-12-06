using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayActive : MonoBehaviour
{
    // Start is called before the first frame update
    public float activeAfter = 0;
    public GameObject activeObj;
    public void Awake()
    {
        if (activeAfter != 0 && activeObj)
        {
            activeObj.SetActive(false);
            StartCoroutine(ActivateAfterDelay());
        }

    }

    IEnumerator ActivateAfterDelay()
    {
        // Wait for the specified delay time
        yield return new WaitForSeconds(activeAfter);

        // Activate the GameObject
        activeObj.SetActive(true);
    }

}

