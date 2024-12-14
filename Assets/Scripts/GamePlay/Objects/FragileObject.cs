using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Represents a fragile object that can break from impacts and pressure
/// Inherits from DraggableObject to handle drag and drop functionality
/// </summary>
public class FragileObject : DraggableObject
{
    #region Serialized Fields
    [Header("Fragile Object Properties")] 
    [SerializeField] private float fragileStrength = 0.5f;           // Base strength of the fragile object

    [Header("Break Conditions")] 
    [SerializeField] private float breakImpactThreshold = 2f;        // Threshold for instant break from impact
    [SerializeField] private float breakForceThreshold = 5f;         // Threshold for break from sustained force
    [SerializeField] private float forceResetTime = 0.5f;           // Time before accumulated force resets
    
    [Header("Break Effects")] 
    [SerializeField] public GameObject brokenPrefab;                 // Prefab spawned when object breaks
    #endregion

    #region Private Fields
    private float accumulatedForce = 0f;                            // Total force accumulated
    private float lastForceTime;                                     // Time of last force application
    private bool isBroken = false;                                   // Current break state
    
    private static List<GameObject> brokenPieces = new List<GameObject>();  // Tracks all broken pieces
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// Initialize the fragile object
    /// </summary>
    protected override void Start()
    {
        base.Start();
        // Increase drag speed to reflect lighter weight
        baseDragSpeed *= 1.2f;
        CalculateDragSpeed();
        
        // Decrease gravity impact
        if (rb != null)
        {
            // Fragile objects fall more lightly
            rb.gravityScale *= 0.8f;  // Adjust this coefficient to change the fall speed of fragile objects
        }
        
        lastForceTime = Time.time;
    }
    #endregion

    #region Object Properties
    /// <summary>
    /// Identifies object as fragile
    /// </summary>
    public override bool IsFragile()
    {
        return true;
    }

    /// <summary>
    /// Returns the fragile strength value
    /// </summary>
    public override float GetStrength()
    {
        return fragileStrength;
    }
    #endregion

    #region Collision Handling
    /// <summary>
    /// Handles initial collision impact
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isBroken || IsDragging) return;

        // Get collision point relative to fragile object
        Vector2 relativePoint = collision.GetContact(0).point - (Vector2)transform.position;
        bool isTopCollision = relativePoint.y > 0;

        float impactForce = collision.GetContact(0).normalImpulse;
        if (isTopCollision)
        {
            // Consider colliding object's mass for top collisions
            impactForce *= (collision.rigidbody?.mass ?? 1f);
        }

        // Output debug information
        // Debug.Log($"Impact force: {impactForce} on {gameObject.name} from {collision.gameObject.name}" +
        //           $" (TopCollision: {isTopCollision}, NormalImpulse: {collision.GetContact(0).normalImpulse})");

        // Check for instant break threshold
        if (impactForce >= breakImpactThreshold)
        {
            Break(collision.GetContact(0).point);
        }
    }

    /// <summary>
    /// Handles sustained collision pressure
    /// </summary>
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isBroken || IsDragging) return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            // Check for pressure from above
            if (contact.point.y > transform.position.y &&
                collision.gameObject.transform.position.y > transform.position.y)
            {
                float appliedForce = contact.normalImpulse;

                float currentTime = Time.time;
                if (currentTime - lastForceTime > forceResetTime)
                {
                    accumulatedForce = 0f;
                }

                accumulatedForce += appliedForce * Time.deltaTime;
                lastForceTime = currentTime;

                // Debug.Log($"Pressure force: {accumulatedForce} on {gameObject.name} from {collision.gameObject.name}" +
                //           $" (NormalForce: {contact.normalImpulse})");

                if (accumulatedForce >= breakForceThreshold)
                {
                    Break(contact.point);
                }

                break;
            }
        }
    }

    /// <summary>
    /// Handles collision exit
    /// </summary>
    private void OnCollisionExit2D(Collision2D collision)
    {
        // Reset accumulated force when top object leaves
        if (collision.gameObject.transform.position.y > transform.position.y)
        {
            accumulatedForce = 0f;
            lastForceTime = Time.time;
        }
    }
    #endregion

    #region Break Handling
    /// <summary>
    /// Handles the breaking of the object
    /// </summary>
    private void Break(Vector2 breakPoint)
    {
        if (isBroken) return;
        isBroken = true;

        // Debug.Log($"{gameObject.name} has broken");
        SoundManager.Instance.PlaySoundFromResources("Sound/1putbreak", "1putbreak", false, 1.0f);

        if (brokenPrefab != null)
        {
            GameObject broken = Instantiate(brokenPrefab, transform.position, transform.rotation);
            brokenPieces.Add(broken);

            // Transfer velocity to broken pieces if possible
            if (rb != null && broken.TryGetComponent<Rigidbody2D>(out var brokenRb))
            {
                brokenRb.velocity = rb.velocity;
                brokenRb.angularVelocity = rb.angularVelocity;
            }
        }

        Destroy(gameObject);
    }
    #endregion

    #region Debug Visualization
    /// <summary>
    /// Draws debug visualization in editor
    /// </summary>
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && accumulatedForce > 0)
        {
            float radius = accumulatedForce / breakForceThreshold * 0.5f;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
    #endregion

    #region Cleanup
    /// <summary>
    /// Cleanup when object is destroyed
    /// </summary>
    protected override void OnDestroy()
    {
        base.OnDestroy();

        // Clean up references
        brokenPrefab = null;
        isBroken = false;
        accumulatedForce = 0f;
    }

    /// <summary>
    /// Static method to clean up all broken pieces
    /// </summary>
    public static void CleanupBrokenPieces()
    {
        foreach (var piece in brokenPieces)
        {
            if (piece != null)
            {
                Destroy(piece);
            }
        }

        brokenPieces.Clear();
    }
    #endregion
}