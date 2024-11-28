using UnityEngine;

public class TranslatorCollectible : CollectibleObject
{
    protected override bool CheckTriggerInteraction(GameObject other)
    {
        if (!isUnlocked && data.unlockMethod == UnlockMethod.Disk)
        {
            return other.CompareTag("Disk");
        }
        return false;
    }
}