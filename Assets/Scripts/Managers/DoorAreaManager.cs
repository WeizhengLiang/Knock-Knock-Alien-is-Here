using UnityEngine;
using System.Collections.Generic;

public class DoorAreaManager : MonoBehaviour
{
    public static DoorAreaManager Instance { get; private set; }
    
    [Header("区域设置")]
    [SerializeField] private DoorCoverageCalculator doorCoverageCalculator;

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
    
    public float GetCurrentCoverage()
    {
        return doorCoverageCalculator != null ? doorCoverageCalculator.GetCurrentCoverage() : 0f;
    }

    public bool IsOnlySpecimenUsedForCoverage()
    {
        // 获取所有在门区域内的物体
        var objectsInDoor = GetObjectsInDoorArea();
        
        // 检查是否只有标本类型的物体
        foreach (var obj in objectsInDoor)
        {
            SpecimenCollectible collectible = obj.GetComponent<SpecimenCollectible>();
            if (collectible == null)
            {
                // 如果发现非标本物体或不是收集品的物体，返回false
                return false;
            }
        }
        
        // 确保至少有一个标本
        return objectsInDoor.Count > 0;
    }

    private List<DraggableObject> GetObjectsInDoorArea()
    {
        List<DraggableObject> objectsInDoor = new List<DraggableObject>();
        
        foreach (var obj in DraggableObject.AllDraggableObjects)
        {
            if (!obj.IsInInvalidPosition() && !obj.IsDragging() && IsObjectInDoorArea(obj))
            {
                objectsInDoor.Add(obj);
            }
        }
        
        return objectsInDoor;
    }

    private bool IsObjectInDoorArea(DraggableObject obj)
    {
        // 使用物理检测来判断物体是否在门区域内
        Collider2D objCollider = obj.GetComponent<Collider2D>();
        Collider2D doorCollider = doorCoverageCalculator.doorArea;
        
        if (objCollider != null && doorCollider != null)
        {
            return objCollider.bounds.Intersects(doorCollider.bounds);
        }
        
        return false;
    }

    private void OnDestroy() 
    {
        if (Instance == this) 
        {
            Instance = null;
        }
    }
}