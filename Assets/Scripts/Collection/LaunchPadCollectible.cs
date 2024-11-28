using UnityEngine;

public class LaunchPadCollectible : CollectibleObject
{
    [SerializeField] private Transform launchPoint;
    [SerializeField] private float checkRadius = 0.5f;

    private bool isRocketMounted = false;
    private Rigidbody2D mountedRocket;
    
    protected override void Update()
    {
        base.Update();
        if (!isUnlocked)
        {
            if (!isRocketMounted)
            {
                CheckRocketMount();
            }
        }
    }
    
    private void CheckRocketMount()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(launchPoint.position, checkRadius);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Rocket"))
            {
                isRocketMounted = true;
                break;
            }
        }
    }
}