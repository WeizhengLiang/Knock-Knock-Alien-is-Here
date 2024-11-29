using UnityEngine;

public class SmoothCameraScroller : MonoBehaviour
{
    public float maxScrollSpeed = 10f; // Maximum scrolling speed
    public float accelerationRate = 5f; // Speed of acceleration
    public float decelerationRate = 8f; // Speed of deceleration
    public float leftBoundary_X = -10f; // Left boundary for the camera
    public float rightBoundary_X = 10f; // Right boundary for the camera

    private float currentScrollSpeed = 0f; // Current speed of the camera
    private Vector3 targetDirection = Vector3.zero; // Direction of movement
    private Camera cam; // Reference to the camera

    void Start()
    {
        cam = Camera.main; // Get the main camera
    }

    void Update()
    {
        HandleInput(); // Check for player input
        UpdateScrollSpeed(); // Smoothly adjust the scrolling speed
        MoveCamera(); // Move the camera based on the current speed
    }

    private void HandleInput()
    {
        // Check for input and set the target direction
        if (Input.GetKey(KeyCode.A))
        {
            targetDirection = Vector3.left; // Move left
        }
        else if (Input.GetKey(KeyCode.D))
        {
            targetDirection = Vector3.right; // Move right
        }
        else
        {
            targetDirection = Vector3.zero; // No movement
        }
    }

    private void UpdateScrollSpeed()
    {
        // Determine the target speed based on the direction
        float targetSpeed = targetDirection == Vector3.left ? -maxScrollSpeed :
                            targetDirection == Vector3.right ? maxScrollSpeed : 0f;

        if (targetDirection == Vector3.zero)
        {
            // Decelerate smoothly to 0 when no input is detected
            currentScrollSpeed = Mathf.MoveTowards(currentScrollSpeed, 0f, decelerationRate * Time.deltaTime);
        }
        else
        {
            // Accelerate smoothly toward the target speed
            currentScrollSpeed = Mathf.MoveTowards(currentScrollSpeed, targetSpeed, accelerationRate * Time.deltaTime);
        }
    }

    private void MoveCamera()
    {
        if (Mathf.Abs(currentScrollSpeed) > 0f)
        {
            // Calculate the new position
            Vector3 newPosition = cam.transform.position + Vector3.right * currentScrollSpeed * Time.deltaTime;

            // Clamp the camera's position to the boundaries
            newPosition.x = Mathf.Clamp(newPosition.x, leftBoundary_X, rightBoundary_X);

            // If the camera hits the boundary, stop the speed
            if (newPosition.x <= leftBoundary_X || newPosition.x >= rightBoundary_X)
            {
                currentScrollSpeed = 0f; // Stop the speed
            }

            // Apply the new position
            cam.transform.position = newPosition;
        }
    }
}







// using UnityEngine;
// using UnityEngine.EventSystems;

// public class CameraScroller : MonoBehaviour
// {
//     public float baseScrollSpeed = 10f; // The base scroll speed
//     private float currentScrollSpeed; // The current scroll speed (increases or decreases over time)
//     private Camera cam;
//     public float leftBoundary_X, rightBoundary_X;
//     private bool shouldScrollLeft = false;
//     private bool shouldScrollRight = false;
//     public GameObject leftScreenEdge, rightScreenEdge;
//     private Animator animLeft, animRight;
//     public float accelerationRate = 12f; // Rate at which the scroll speed increases
//     public float decelerationRate = 30f; // Rate at which the scroll speed decreases

//     private Vector3 scrollDirection = Vector3.zero; // The current scrolling direction

//     void Start()
//     {
//         cam = Camera.main;
//         if (leftScreenEdge != null) animLeft = leftScreenEdge.GetComponent<Animator>();
//         if (rightScreenEdge != null) animRight = rightScreenEdge.GetComponent<Animator>();
//         currentScrollSpeed = baseScrollSpeed; // Initialize the current speed to the base speed
//     }

//     void Update()
//     {
//         // Check for A and D key inputs to determine scroll direction
//         if (Input.GetKey(KeyCode.A))
//         {
//             AccelerateScrollSpeed(); // Gradually increase speed
//             scrollDirection = Vector3.left; // Set the direction to left
//         }
//         else if (Input.GetKey(KeyCode.D))
//         {
//             AccelerateScrollSpeed(); // Gradually increase speed
//             scrollDirection = Vector3.right; // Set the direction to right
//         }
//         // Mouse hover logic (commented out to disable it)
//         /*
//         else if (shouldScrollLeft)
//         {
//             AccelerateScrollSpeed(); // Gradually increase speed
//             scrollDirection = Vector3.left; // Set the direction to left
//         }
//         else if (shouldScrollRight)
//         {
//             AccelerateScrollSpeed(); // Gradually increase speed
//             scrollDirection = Vector3.right; // Set the direction to right
//         }
//         */
//         else
//         {
//             DecelerateScrollSpeed(); // Gradually decrease speed
//         }

//         // Move the camera smoothly in the current direction
//         MoveCamera(scrollDirection);
//     }

//     private void MoveCamera(Vector3 direction)
//     {
//         if (direction == Vector3.zero || currentScrollSpeed <= 0f)
//             return; // Stop moving if there's no direction or the speed is too slow

//         // Calculate the new position based on the direction and current scroll speed
//         Vector3 newPosition = cam.transform.position + direction * currentScrollSpeed * Time.deltaTime;

//         // Clamp the camera's position to the boundaries
//         newPosition.x = Mathf.Clamp(newPosition.x, leftBoundary_X, rightBoundary_X);

//         // Apply the new position
//         cam.transform.position = newPosition;
//     }

//     private void AccelerateScrollSpeed()
//     {
//         // Gradually increase the scroll speed
//         currentScrollSpeed += accelerationRate * Time.deltaTime;
//     }

//     private void DecelerateScrollSpeed()
//     {
//         // Gradually decrease the scroll speed until it reaches the base speed
//         if (currentScrollSpeed > baseScrollSpeed)
//         {
//             currentScrollSpeed -= decelerationRate * Time.deltaTime;
//         }
//         else
//         {
//             currentScrollSpeed = baseScrollSpeed; // Clamp to the base speed
//             scrollDirection = Vector3.zero; // Stop scrolling when deceleration is complete
//         }
//     }

//     // Mouse hover logic (commented out to disable it)
//     /*
//     public void PointerEnter(string side)
//     {
//         if (side == "left")
//         {
//             shouldScrollLeft = true;
//             if (animLeft != null) animLeft.SetBool("Show", true);
//         }
//         else if (side == "right")
//         {
//             shouldScrollRight = true;
//             if (animRight != null) animRight.SetBool("Show", true);
//         }
//     }

//     public void PointerExit(string side)
//     {
//         if (side == "left")
//         {
//             shouldScrollLeft = false;
//             if (animLeft != null) animLeft.SetBool("Show", false);
//         }
//         else if (side == "right")
//         {
//             shouldScrollRight = false;
//             if (animRight != null) animRight.SetBool("Show", false);
//         }
//     }
//     */
// }
