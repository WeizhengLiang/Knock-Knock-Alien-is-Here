using UnityEngine;

[CreateAssetMenu(fileName = "New Collectible", menuName = "Game/Collectible")]
public class CollectibleData : ScriptableObject
{
    [Header("Basic Info")]
    public CollectibleType type;
    public string itemName;
    public string description;
    public string alienResponse;
    public UnlockMethod unlockMethod;
    
    [Header("Visual Elements")]
    public Sprite icon;                    // 书架上显示的图标
    public Sprite unlockedSprite;          // 解锁状态的图片
    public Sprite lockedSprite;            // 未解锁状态的图片
    public Sprite comicSprite;             // 右侧显示的漫画图片
    
    [Header("Effects")]
    public GameObject unlockEffectPrefab;
    public AudioClip unlockSound;
}

public enum CollectibleType
{
    Declaration,    // 星际友好宣言
    LaserPointer,  // 超规格激光指针
    Wiretapper,    // 超远距离窃听器
    Translator,    // 语言消化机器
    LaunchPad,     // 隐藏式发射台
    Specimen       // 未命名的囚徒
}

public enum UnlockMethod
{
    Drop,           // 摔落解锁
    PowerSource,    // 需要电源
    Position,       // 位置解锁
    Disk,          // 需要软盘
    Rocket,        // 需要火箭
    Break          // 需要摔碎
}