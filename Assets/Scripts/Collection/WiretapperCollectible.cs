// 超远距离窃听器（位置解锁）

using UnityEngine;

public class WiretapperCollectible : CollectibleObject
{
    [SerializeField] private Transform targetPosition;
    [SerializeField] private float acceptableDistance = 0.5f;
    
    protected override void Update()
    {
        base.Update();
        if (!isUnlocked)
        {
            CheckPosition();
        }
    }
    
    private void CheckPosition()
    {
        float distance = Vector2.Distance(transform.position, targetPosition.position);
        if (distance <= acceptableDistance)
        {
            Unlock();
        }
    }
}