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
        
        isUnlocked = PlayerPrefs.GetInt($"Collectible_{data.type}", 0) == 1;

        if (isUnlocked && unlockVisualEffect != null)
        {
            unlockVisualEffect.SetActive(true);
        }

        var collider = GetComponent<Collider2D>();
        collider.isTrigger = true;
    }

    public bool CanTrigger(GameObject trigger)
    {
        return trigger.CompareTag("Rocket");
    }

    public void OnTriggerStart(GameObject trigger)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.material = canMergeMaterial;
        }
    }

    public void OnTriggerEnd(GameObject trigger)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.material = normalMaterial;
        }
    }

    public void OnTriggerComplete(GameObject trigger)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.material = normalMaterial;
        }

        if (unlockVisualEffect != null)
        {
            unlockVisualEffect.SetActive(true);
        }

        if (!isUnlocked)
        {
            isUnlocked = true;
            PlayerPrefs.SetInt($"Collectible_{data.type}", 1);
            PlayerPrefs.Save();
            CollectibleManager.Instance.UnlockCollectible(data.type);
        }

        if (trigger != null)
        {
            Destroy(trigger);
        }
        SoundManager.Instance.PlaySoundFromResources("Sound/3-5launch", "3-5launch", false, 1.0f);
    }
}