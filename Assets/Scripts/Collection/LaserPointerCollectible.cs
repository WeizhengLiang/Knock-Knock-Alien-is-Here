// 超规格激光指针（需要电源）

using UnityEngine;

public class LaserPointerCollectible : CollectibleObject
{
    [SerializeField] private Transform powerSourceSocket;
    [SerializeField] private float checkRadius = 0.5f;
    [SerializeField] private LayerMask powerSourceLayer;
    
    protected override void Update()
    {
        base.Update();
        if (!isUnlocked)
        {
            CheckPowerSource();
        }
    }
    
    private void CheckPowerSource()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(powerSourceSocket.position, checkRadius, powerSourceLayer);
        if (colliders.Length > 0)
        {
            Unlock();
        }
    }
}