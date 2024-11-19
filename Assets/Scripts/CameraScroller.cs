using UnityEngine;
using UnityEngine.EventSystems;

public class CameraScroller : MonoBehaviour
{
    public float scrollSpeed = 5f;
    private Camera cam;
    public float leftBoundary_X, rightBoundary_X;
    private bool shouldScrollLeft = false;
    private bool shouldScrollRight = false;

    void Start()
    {
        cam = Camera.main;
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
        //Debug.Log(side);
        if (side == "left")
        {
            shouldScrollLeft = true;
        }
        else if (side == "right")
        {
            shouldScrollRight = true;
        }
    }

    public void PointerExit(string side)
    {
        //Debug.Log(side);
        if (side == "left")
        {
            shouldScrollLeft = false;
        }
        else if (side == "right")
        {
            shouldScrollRight = false;
        }
    }
}


