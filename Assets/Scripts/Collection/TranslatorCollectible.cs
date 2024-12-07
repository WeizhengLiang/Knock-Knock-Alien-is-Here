using UnityEngine;

public class TranslatorCollectible : CollectibleObject
{
    protected override bool CheckTriggerInteraction(GameObject other)
    {
        if (data.unlockMethod == UnlockMethod.Disk && other.CompareTag("Disk"))
        {
            return true;
        }
        return false;
    }

    protected override void Unlock()
    {
        if (!isUnlocked)
        {
            var spriteSwitch = GetComponent<SpriteSwitch>();
            base.Unlock();
            spriteSwitch.SwitchToSecondList();
        }
    }
}