using UnityEngine;

public class LaunchPadCollectible : CollectibleObject
{
    [SerializeField] private LayerMask rocketLayer;
    [SerializeField] private Transform launchPoint;
    [SerializeField] private float checkRadius = 0.5f;
    [SerializeField] private ParticleSystem launchEffect;
    [SerializeField] private float launchForce = 10f;
    
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
            else if (!mountedRocket.isKinematic)
            {
                LaunchRocket();
            }
        }
    }
    
    private void CheckRocketMount()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(launchPoint.position, checkRadius, rocketLayer);
        foreach (var collider in colliders)
        {
            mountedRocket = collider.GetComponent<Rigidbody2D>();
            if (mountedRocket != null)
            {
                isRocketMounted = true;
                break;
            }
        }
    }
    
    private void LaunchRocket()
    {
        if (launchEffect != null)
        {
            launchEffect.Play();
        }
        mountedRocket.AddForce(Vector2.up * launchForce, ForceMode2D.Impulse);
        Unlock();
    }
}