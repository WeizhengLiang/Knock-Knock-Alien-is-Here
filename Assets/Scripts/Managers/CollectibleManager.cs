using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CollectibleManager : MonoBehaviour
{
    public static CollectibleManager Instance { get; private set; }
    
    [Header("Collection Data")]
    [SerializeField] private CollectibleData[] collectibleDatabase;
    
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
    
    public void InitializeCollection()
    {
        foreach (var data in collectibleDatabase)
        {
            unlockedCollectibles[data.type] = false;
        }
        LoadCollectionState();
    }
    
    // 解锁收集物并设置新收集物标记
    public void UnlockCollectible(CollectibleType type)
    {
        if (!unlockedCollectibles[type])
        {
            unlockedCollectibles[type] = true;
            PlayerPrefs.SetInt("HasNewCollectible", 1);
            SaveCollectionState();
        }
    }
    
    public bool IsCollectibleUnlocked(CollectibleType type)
    {
        return unlockedCollectibles.ContainsKey(type) && unlockedCollectibles[type];
    }
    
    public bool HasNewCollectible()
    {
        return PlayerPrefs.GetInt("HasNewCollectible", 0) == 1;
    }
    
    private void SaveCollectionState()
    {
        foreach (var pair in unlockedCollectibles)
        {
            PlayerPrefs.SetInt($"Collectible_{pair.Key}", pair.Value ? 1 : 0);
        }
        PlayerPrefs.Save();
    }
    
    private void LoadCollectionState()
    {
        foreach (var data in collectibleDatabase)
        {
            int state = PlayerPrefs.GetInt($"Collectible_{data.type}", 0);
            unlockedCollectibles[data.type] = state == 1;
        }
    }
    
    // 获取收集物数据
    public CollectibleData GetCollectibleData(CollectibleType type)
    {
        return collectibleDatabase.FirstOrDefault(data => data.type == type);
    }
    
    // 公共属性
    public CollectibleData[] CollectibleDatabase => collectibleDatabase;
    public Dictionary<CollectibleType, bool> UnlockedCollectibles => unlockedCollectibles;
}