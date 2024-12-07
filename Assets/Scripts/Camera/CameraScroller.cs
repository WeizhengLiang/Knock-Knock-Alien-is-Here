using UnityEngine;

public class SmoothCameraScroller : MonoBehaviour
{
    public static SmoothCameraScroller Instance { get; private set; }

    [Header("Scroll Settings")]
    public float maxScrollSpeed = 10f;
    public float accelerationRate = 5f;
    public float decelerationRate = 8f;
    public float leftBoundary_X = -10f;
    public float rightBoundary_X = 10f;

    private float currentScrollSpeed = 0f;
    private Vector3 targetDirection = Vector3.zero;
    private Camera cam;
    private bool isLocked = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        cam = Camera.main;
    }

    public void SetCameraLocked(bool locked)
    {
        isLocked = locked;
        if (locked)
        {
            // 立即停止移动
            currentScrollSpeed = 0f;
            targetDirection = Vector3.zero;
        }
    }

    void Update()
    {
        if (!isLocked)
        {
            HandleInput();
            UpdateScrollSpeed();
            MoveCamera();
        }
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
