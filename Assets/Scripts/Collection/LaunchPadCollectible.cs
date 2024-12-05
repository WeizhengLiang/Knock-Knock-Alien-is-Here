using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LaunchPadCollectible : MonoBehaviour, ITriggerable
{
    [SerializeField] private CollectibleData data;
    [SerializeField] private GameObject unlockVisualEffect;
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material canMergeMaterial;

    private SpriteRenderer spriteRenderer;
    private bool isUnlocked;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.material = normalMaterial;
        
        // 从 PlayerPrefs 初始化 isUnlocked
        isUnlocked = PlayerPrefs.GetInt($"Collectible_{data.type}", 0) == 1;

        // 如果已解锁，激活解锁视觉效果
        if (isUnlocked && unlockVisualEffect != null)
        {
            unlockVisualEffect.SetActive(true);
        }

        // 确保碰撞器设置为触发器
        var collider = GetComponent<Collider2D>();
        collider.isTrigger = true;
    }

    public bool CanTrigger(GameObject trigger)
    {
        return !isUnlocked && trigger.CompareTag("Rocket");
    }

    public void OnTriggerStart(GameObject trigger)
    {
        spriteRenderer.material = canMergeMaterial;
    }

    public void OnTriggerEnd(GameObject trigger)
    {
        spriteRenderer.material = normalMaterial;
    }

    public void OnTriggerComplete(GameObject trigger)
    {
        isUnlocked = true;
        spriteRenderer.material = normalMaterial;
        
        if (unlockVisualEffect != null)
        {
            unlockVisualEffect.SetActive(true);
        }

        // 更新 PlayerPrefs
        PlayerPrefs.SetInt($"Collectible_{data.type}", 1);
        PlayerPrefs.Save();

        CollectibleManager.Instance.UnlockCollectible(data.type);

        // 让 Rocket 消失
        if (trigger != null)
        {
            Destroy(trigger);
        }
    }
}