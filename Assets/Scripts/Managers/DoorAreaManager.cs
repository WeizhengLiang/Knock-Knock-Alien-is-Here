using UnityEngine;

public class DoorAreaManager : MonoBehaviour
{
    public static DoorAreaManager Instance { get; private set; }
    
    [Header("区域设置")]
    [SerializeField] private BoxCollider2D doorArea; // 门口可放置区域
    [SerializeField] private float maxStackHeight = 5f; // 最大堆叠高度
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public bool IsInsideDoorArea(Vector2 position)
    {
        return doorArea.bounds.Contains(position);
    }
    
    public float GetMaxStackHeight()
    {
        return maxStackHeight;
    }
}