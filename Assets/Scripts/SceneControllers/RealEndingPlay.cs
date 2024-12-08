using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class RealEndingPlay : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<PlayableDirector>().Play();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
