using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoldUIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // Time in seconds the button must be held
    public float holdTime = 1f;
    public Image SkipBar;

    // Tracks how long the button has been held
    private float holdTimer = 0f;

    // Flag to check if the button is being held
    private bool isHolding = false;
    public UnityEvent OnButtonHeld;


    // Update is called once per frame
    void Update()
    {
        // If the button is being held, increment the timer
        if (isHolding)
        {
            holdTimer += Time.deltaTime;
            SetSkipBar();

            // If the hold time is reached, trigger the action
            if (holdTimer >= holdTime)
            {
                isHolding = false; // Prevent further calls
                OnButtonHeld?.Invoke();  // Call the function
            }
        }
    }

    // Called when the button is pressed down
    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding = true;
        holdTimer = 0f; // Reset the timer
    }

    // Called when the button is released
    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
        holdTimer = 0f; // Reset the timer
        SetSkipBar();
    }

    private void SetSkipBar()
    {
        SkipBar.fillAmount = holdTimer / holdTime;
    }

    // Function to be called when the button is held for the required time
}
