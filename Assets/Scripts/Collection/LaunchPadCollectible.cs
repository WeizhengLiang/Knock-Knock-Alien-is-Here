using UnityEngine;

public class LaunchPadCollectible : CollectibleObject
{
    protected override bool CheckTriggerInteraction(GameObject other)
    {
        if (!isUnlocked && data.unlockMethod == UnlockMethod.Rocket)
        {
            return other.CompareTag("Rocket");
        }
        return false;
    }
}