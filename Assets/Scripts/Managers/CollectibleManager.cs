using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CollectibleManager : MonoBehaviour
{
    public static CollectibleManager Instance { get; private set; }
    
    [Header("Collection Data")]
    [SerializeField] private CollectibleData[] collectibleDatabase;
    
    [Header("UI References")]
    [SerializeField] private GameObject unlockEffectPrefab;
    [SerializeField] private AudioSource audioSource;
    
    private Dictionary<CollectibleType, bool> unlockedCollectibles = new Dictionary<CollectibleType, bool>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeCollection();
        }
        else
        {
            Destroy(gameObject);
        }
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

            SaveCollectionState();
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
    
    // 添加公共属性
    public CollectibleData[] CollectibleDatabase => collectibleDatabase;
    public Dictionary<CollectibleType, bool> UnlockedCollectibles => unlockedCollectibles;
}