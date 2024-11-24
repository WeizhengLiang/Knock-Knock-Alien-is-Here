using UnityEngine;
using UnityEngine.EventSystems;

public class CameraScroller : MonoBehaviour
{
    public float baseScrollSpeed = 10f; // The base scroll speed
    private float currentScrollSpeed; // The current scroll speed (increases or decreases over time)
    private Camera cam;
    public float leftBoundary_X, rightBoundary_X;
    private bool shouldScrollLeft = false;
    private bool shouldScrollRight = false;
    public GameObject leftScreenEdge, rightScreenEdge;
    private Animator animLeft, animRight;
    public float accelerationRate = 12f; // Rate at which the scroll speed increases
    public float decelerationRate = 30f; // Rate at which the scroll speed decreases

    private Vector3 scrollDirection = Vector3.zero; // The current scrolling direction

    void Start()
    {
        cam = Camera.main;
        animLeft = leftScreenEdge.GetComponent<Animator>();
        animRight = rightScreenEdge.GetComponent<Animator>();
        currentScrollSpeed = baseScrollSpeed; // Initialize the current speed to the base speed
    }

    void Update()
    {
        if (shouldScrollLeft)
        {
            AccelerateScrollSpeed(); // Gradually increase speed
            scrollDirection = Vector3.left; // Set the direction to left
        }
        else if (shouldScrollRight)
        {
            AccelerateScrollSpeed(); // Gradually increase speed
            scrollDirection = Vector3.right; // Set the direction to right
        }
        else
        {
            DecelerateScrollSpeed(); // Gradually decrease speed
        }

        // Move the camera smoothly in the current direction
        MoveCamera(scrollDirection);
    }

    private void MoveCamera(Vector3 direction)
    {
        if (direction == Vector3.zero || currentScrollSpeed <= 0f)
            return; // Stop moving if there's no direction or the speed is too slow

        // Calculate the new position based on the direction and current scroll speed
        Vector3 newPosition = cam.transform.position + direction * currentScrollSpeed * Time.deltaTime;

        // Clamp the camera's position to the boundaries
        newPosition.x = Mathf.Clamp(newPosition.x, leftBoundary_X, rightBoundary_X);

        // Apply the new position
        cam.transform.position = newPosition;
    }

    private void AccelerateScrollSpeed()
    {
        // Gradually increase the scroll speed
        currentScrollSpeed += accelerationRate * Time.deltaTime;
    }

    private void DecelerateScrollSpeed()
    {
        // Gradually decrease the scroll speed until it reaches the base speed
        if (currentScrollSpeed > baseScrollSpeed)
        {
            currentScrollSpeed -= decelerationRate * Time.deltaTime;
        }
        else
        {
            currentScrollSpeed = baseScrollSpeed; // Clamp to the base speed
            scrollDirection = Vector3.zero; // Stop scrolling when deceleration is complete
        }
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