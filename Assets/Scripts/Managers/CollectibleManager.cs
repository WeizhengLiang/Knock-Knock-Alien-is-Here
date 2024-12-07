using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CollectibleManager : MonoBehaviour
{
    public static CollectibleManager Instance { get; private set; }
    
    [Header("Collection Data")]
    [SerializeField] private CollectibleData[] collectibleDatabase;
    
    private Dictionary<CollectibleType, bool> unlockedCollectibles = new Dictionary<CollectibleType, bool>();
    private HashSet<CollectibleType> newlyUnlockedCollectibles = new HashSet<CollectibleType>();
    private bool hasViewedNewCollectibles = true;
    
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
        LoadViewedStatus();
    }
    
    public void UnlockCollectible(CollectibleType type)
    {
        if (!IsCollectibleUnlocked(type))
        {
            unlockedCollectibles[type] = true;
            newlyUnlockedCollectibles.Add(type);
            hasViewedNewCollectibles = false;
            SaveCollectionState();
            SaveViewedStatus();
        }
    }
    
    public bool IsCollectibleUnlocked(CollectibleType type)
    {
        return unlockedCollectibles.ContainsKey(type) && unlockedCollectibles[type];
    }
    
    public HashSet<CollectibleType> GetNewlyUnlockedCollectibles()
    {
        return newlyUnlockedCollectibles;
    }
    
    public bool HasNewUnlocksThisSession()
    {
        return newlyUnlockedCollectibles.Count > 0;
    }
    
    public bool HasUnviewedCollectibles()
    {
        return !hasViewedNewCollectibles;
    }
    
    public void MarkCollectiblesAsViewed()
    {
        hasViewedNewCollectibles = true;
        SaveViewedStatus();
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
    
    private void SaveViewedStatus()
    {
        PlayerPrefs.SetInt("HasViewedNewCollectibles", hasViewedNewCollectibles ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    public void LoadViewedStatus()
    {
        hasViewedNewCollectibles = PlayerPrefs.GetInt("HasViewedNewCollectibles", 1) == 1;
    }
    
    private void OnApplicationQuit()
    {
        SaveViewedStatus();
    }
    
    public void ClearNewlyUnlocked()
    {
        newlyUnlockedCollectibles.Clear();
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