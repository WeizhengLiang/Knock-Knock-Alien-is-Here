using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CollectibleManager : MonoBehaviour
{
    public static CollectibleManager Instance { get; private set; }
    
    [Header("Collection Data")]
    [SerializeField] private CollectibleData[] collectibleDatabase;
    
    [Header("UI References")]
    [SerializeField] private CollectionPanel collectionPanel;
    [SerializeField] private GameObject unlockEffectPrefab;
    [SerializeField] private AudioSource audioSource;
    
    private Dictionary<CollectibleType, bool> unlockedCollectibles = new Dictionary<CollectibleType, bool>();
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        InitializeCollection();
    }
    
    private void InitializeCollection()
    {
        foreach (var data in collectibleDatabase)
        {
            unlockedCollectibles[data.type] = false;
        }
        LoadCollectionState();
    }
    
    public void UnlockCollectible(CollectibleType type)
    {
        if (!unlockedCollectibles[type])
        {
            unlockedCollectibles[type] = true;
            
            // 播放解锁特效和音效
            CollectibleData data = GetCollectibleData(type);
            if (data != null)
            {
                if (data.unlockSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(data.unlockSound);
                }
                if (data.unlockEffectPrefab != null)
                {
                    Instantiate(data.unlockEffectPrefab, transform.position, Quaternion.identity);
                }
            }
            
            // 更新UI并保存状态
            UpdateCollectionUI();
            SaveCollectionState();
        }
    }
    
    public void ShowCollectionPanel()
    {
        if (collectionPanel != null)
        {
            collectionPanel.gameObject.SetActive(true);
            collectionPanel.SetupCollectionItems(collectibleDatabase, unlockedCollectibles);
        }
    }
    
    private void UpdateCollectionUI()
    {
        if (collectionPanel != null && collectionPanel.gameObject.activeInHierarchy)
        {
            collectionPanel.SetupCollectionItems(collectibleDatabase, unlockedCollectibles);
        }
    }
    
    private CollectibleData GetCollectibleData(CollectibleType type)
    {
        return collectibleDatabase.FirstOrDefault(data => data.type == type);
    }
    
    private void SaveCollectionState()
    {
        foreach (var pair in unlockedCollectibles)
        {
            PlayerPrefs.SetInt($"Collectible_{pair.Key}", pair.Value ? 1 : 0);
        }
        PlayerPrefs.Save();
    }
    
    public void LoadCollectionState()
    {
        foreach (var data in collectibleDatabase)
        {
            int state = PlayerPrefs.GetInt($"Collectible_{data.type}", 0);
            unlockedCollectibles[data.type] = state == 1;
        }
    }
}