// 超规格激光指针（需要电源）

using UnityEngine;

public class LaserPointerCollectible : CollectibleObject
{
    protected override bool CheckTriggerInteraction(GameObject other)
    {
        if (!isUnlocked && data.unlockMethod == UnlockMethod.PowerSource)
        {
            return other.CompareTag("PowerSource");
        }
        return false;
    }
}