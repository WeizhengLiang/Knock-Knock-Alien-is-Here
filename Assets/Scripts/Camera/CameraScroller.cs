using UnityEngine;
using UnityEngine.EventSystems;

public class CameraScroller : MonoBehaviour
{
    public float scrollSpeed = 5f;
    private Camera cam;
    public float leftBoundary_X, rightBoundary_X;
    private bool shouldScrollLeft = false;
    private bool shouldScrollRight = false;
    public GameObject leftScreenEdge, rightScreenEdge;
    private Animator animLeft, animRight;

    void Start()
    {
        cam = Camera.main;
        animLeft = leftScreenEdge.GetComponent<Animator>();
        animRight = rightScreenEdge.GetComponent<Animator>();
    }

    void Update()
    {
        if (shouldScrollLeft)
        {
            MoveCamera(Vector3.left);
        }
        else if (shouldScrollRight)
        {
            MoveCamera(Vector3.right);
        }
    }

    private void MoveCamera(Vector3 direction)
    {
        Vector3 newPosition = cam.transform.position + direction * scrollSpeed * Time.deltaTime;

        // Clamp the camera's position to the boundaries
        newPosition.x = Mathf.Clamp(newPosition.x, leftBoundary_X, rightBoundary_X);
        cam.transform.position = newPosition;
    }


    public void PointerEnter(string side)
    {
        if (side == "left")
        {
            shouldScrollLeft = true;
            animLeft.SetBool("Show", true);
        }
        else if (side == "right")
        {
            shouldScrollRight = true;
            animRight.SetBool("Show", true);
        }
    }

    public void PointerExit(string side)
    {
        if (side == "left")
        {
            shouldScrollLeft = false;
            animLeft.SetBool("Show", false);
        }
        else if (side == "right")
        {
            shouldScrollRight = false;
            animRight.SetBool("Show", false);
        }
    }
}


