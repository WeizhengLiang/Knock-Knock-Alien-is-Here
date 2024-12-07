// 超规格激光指针（需要电源）

using UnityEngine;

public class LaserPointerCollectible : CollectibleObject
{
    [Header("Power Settings")]
    [SerializeField] private Sprite unpoweredSprite;
    [SerializeField] private Sprite poweredSprite;

    protected override void Start()
    {
        base.Start();

        // 根据解锁状态设置初始精灵
        if (IsUnlocked())
        {
            spriteRenderer.sprite = poweredSprite;
        }
        else
        {
            spriteRenderer.sprite = unpoweredSprite;
        }
    }

    protected override bool CheckTriggerInteraction(GameObject other)
    {
        return data.unlockMethod == UnlockMethod.PowerSource && other.CompareTag("PowerSource");
    }

    public override void HandleTriggerEffect()
    {
        // 播放通电效果
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = poweredSprite;
        }

        // 只在未解锁时更新解锁状态
        if (!isUnlocked)
        {
            base.HandleTriggerEffect();
        }
    }
}