using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSwitch : MonoBehaviour
{
    // Public list of sprites for the first set
    public List<Sprite> firstSpriteList = new List<Sprite>();

    // Public list of sprites for the second set
    public List<Sprite> secondSpriteList = new List<Sprite>();

    // Public switching speed (in seconds)
    public float switchSpeed = 1.0f;

    // Reference to the SpriteRenderer component
    private SpriteRenderer spriteRenderer;

    // Internal index to track the current sprite
    private int currentSpriteIndex = 0;

    // Current active sprite list
    private List<Sprite> currentSpriteList;

    // Coroutine reference to stop and restart the switching
    private Coroutine spriteSwitchingCoroutine;

    void Start()
    {
        // Get the SpriteRenderer component attached to this GameObject
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Set the default sprite list to the first list
        currentSpriteList = firstSpriteList;

        // Start the coroutine to switch sprites
        if (currentSpriteList.Count > 0)
        {
            spriteSwitchingCoroutine = StartCoroutine(SwitchSprites());
        }
        else
        {
            Debug.LogWarning("No sprites assigned in the first list.");
        }
    }

    IEnumerator SwitchSprites()
    {
        while (true)
        {
            // Assign the current sprite to the SpriteRenderer
            if (currentSpriteList[currentSpriteIndex] != null)
            {
                spriteRenderer.sprite = currentSpriteList[currentSpriteIndex];
            }

            // Move to the next sprite (looping back to the start if necessary)
            currentSpriteIndex = (currentSpriteIndex + 1) % currentSpriteList.Count;

            // Wait for the specified switch speed
            yield return new WaitForSeconds(switchSpeed);
        }
    }

    // Public method to switch to the second sprite list
    public void SwitchToSecondList()
    {
        // Stop the current coroutine
        if (spriteSwitchingCoroutine != null)
        {
            StopCoroutine(spriteSwitchingCoroutine);
        }

        // Switch to the second sprite list
        currentSpriteList = secondSpriteList;
        currentSpriteIndex = 0; // Reset the index

        // Start the coroutine again if the new list has sprites
        if (currentSpriteList.Count > 0)
        {
            spriteSwitchingCoroutine = StartCoroutine(SwitchSprites());
        }
        else
        {
            Debug.LogWarning("No sprites assigned in the second list.");
        }
    }

    // Public method to switch back to the first sprite list
    public void SwitchToFirstList()
    {
        // Stop the current coroutine
        if (spriteSwitchingCoroutine != null)
        {
            StopCoroutine(spriteSwitchingCoroutine);
        }

        // Switch to the first sprite list
        currentSpriteList = firstSpriteList;
        currentSpriteIndex = 0; // Reset the index

        // Start the coroutine again if the new list has sprites
        if (currentSpriteList.Count > 0)
        {
            spriteSwitchingCoroutine = StartCoroutine(SwitchSprites());
        }
        else
        {
            Debug.LogWarning("No sprites assigned in the first list.");
        }
    }
}