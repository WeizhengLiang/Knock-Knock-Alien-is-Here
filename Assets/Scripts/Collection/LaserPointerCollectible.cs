// 超规格激光指针（需要电源）

using UnityEngine;

public class LaserPointerCollectible : CollectibleObject
{
    [SerializeField] private Transform powerSourceSocket;
    [SerializeField] private float checkRadius = 0.5f;
    
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
        Collider2D[] colliders = Physics2D.OverlapCircleAll(powerSourceSocket.position, checkRadius);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("PowerSource"))
            {
                Unlock();
                break;
            }
        }
    }
}