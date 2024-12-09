// 超远距离窃听器（位置解锁）

using UnityEngine;

/// <summary>
/// Wiretapper collectible that detects vertical area position and controls VFX based on direction
/// </summary>
public class WiretapperCollectible : CollectibleObject
{
    [Header("Detection Settings")]
    [SerializeField] private Transform targetArea;                // The area to detect
    [SerializeField] private float acceptableDistance = 0.5f;    // Distance to trigger unlock
    [SerializeField] private float detectionRange = 5f;          // Range to show directional VFX

    [Header("VFX References")]
    [SerializeField] private GameObject leftVFX;                 // VFX for left side detection
    [SerializeField] private GameObject rightVFX;                // VFX for right side detection

    protected override void Start()
    {
        base.Start();
        // Ensure VFX are initially disabled
        SetVFXState(false, false);
    }

    protected override void Update()
    {
        base.Update();
        // 允许已解锁的窃听器继续检测位置
        CheckAreaPosition();
    }

    /// <summary>
    /// Check area position relative to collectible and update VFX accordingly
    /// </summary>
    private void CheckAreaPosition()
    {
        if (targetArea == null) return;

        Vector2 toArea = targetArea.position - transform.position;
        float distance = toArea.magnitude;

        // Check if within detection range
        if (distance <= detectionRange)
        {
            // Check horizontal position relative to collectible
            if (Mathf.Abs(toArea.x) <= acceptableDistance)
            {
                // Area is vertically aligned - show both VFX
                SetVFXState(true, true);
                SoundManager.Instance.PlaySoundFromResources("Sound/5End3-Alien1", "5End3-Alien1", false, 1.0f);
                // 只在未解锁且距离合适时解锁
                if (!isUnlocked && distance <= acceptableDistance)
                {
                    Unlock();
                }
            }
            else if (toArea.x < 0)
            {
                // Area is to the left
                SetVFXState(true, false);
            }
            else
            {
                // Area is to the right
                SetVFXState(false, true);
            }
            SoundManager.Instance.PlaySoundFromResources("Sound/Searching", "Searching", true, 1.0f);
        }
        else
        {
            // Out of range - hide both VFX
            SetVFXState(false, false);
            SoundManager.Instance.StopSound("Searching");
        }
    }

    /// <summary>
    /// Set the state of both VFX objects
    /// </summary>
    private void SetVFXState(bool leftActive, bool rightActive)
    {
        if (leftVFX != null) leftVFX.SetActive(leftActive);
        if (rightVFX != null) rightVFX.SetActive(rightActive);
    }

    /// <summary>
    /// Draw detection range and zones in editor
    /// </summary>
    private void OnDrawGizmos()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw unlock zone
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, acceptableDistance);
    }
}