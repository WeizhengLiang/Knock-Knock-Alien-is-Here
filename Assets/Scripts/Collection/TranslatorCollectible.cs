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
        SoundManager.Instance.PlaySoundFromResources("Sound/Computing", "Computing", true, 1.0f);
        if (!isUnlocked)
        {
            var spriteSwitch = GetComponent<SpriteSwitch>();
            base.Unlock();
            spriteSwitch.SwitchToSecondList();
        }
    }
}