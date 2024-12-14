using System.Collections.Generic;
using UnityEngine;

public class DraggableObjectManager : MonoBehaviour
{
    private static DraggableObjectManager instance;
    public static DraggableObjectManager Instance 
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<DraggableObjectManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("DraggableObjectManager");
                    instance = go.AddComponent<DraggableObjectManager>();
                }
            }
            return instance;
        }
    }

    private List<DraggableObject> activeObjects = new List<DraggableObject>();
    public IReadOnlyList<DraggableObject> AllDraggableObjects => activeObjects;
    private bool isAnyObjectDragging;
    private bool isGlobalFrozen;
    private bool hasInvalidPlacement;
    private DraggableObject currentDraggingObject;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void RegisterObject(DraggableObject obj)
    {
        if (!activeObjects.Contains(obj))
        {
            activeObjects.Add(obj);
        }
    }

    public void UnregisterObject(DraggableObject obj)
    {
        activeObjects.Remove(obj);
    }

    public void SetDragging(bool isDragging, DraggableObject draggedObject = null)
    {
        if (isAnyObjectDragging != isDragging)
        {
            isAnyObjectDragging = isDragging;
            currentDraggingObject = isDragging ? draggedObject : null;
            
            if (isDragging)
            {
                FreezeAllObjects();
            }
            else
            {
                UnfreezeAllObjects();
            }
        }
    }

    public void SetInvalidPlacement(bool hasInvalid)
    {
        hasInvalidPlacement = hasInvalid;
    }

    public void SetGlobalFrozen(bool frozen)
    {
        if (isGlobalFrozen != frozen)
        {
            isGlobalFrozen = frozen;
            if (frozen)
            {
                FreezeAllObjects();
            }
            else
            {
                UnfreezeAllObjects();
            }
        }
    }

    private void FreezeAllObjects()
    {
        foreach (var obj in activeObjects)
        {
            if (obj != null && obj.enabled && obj != currentDraggingObject)
            {
                obj.PhysicsSystem.Freeze();
            }
        }
    }

    private void UnfreezeAllObjects()
    {
        if (!isGlobalFrozen && !isAnyObjectDragging)
        {
            foreach (var obj in activeObjects)
            {
                if (obj != null && obj.enabled)
                {
                    obj.PhysicsSystem.Unfreeze();
                }
            }
        }
    }

    public bool IsAnyObjectDragging() => isAnyObjectDragging;
    public bool IsGlobalFrozen() => isGlobalFrozen;
    public bool HasInvalidPlacement() => hasInvalidPlacement;

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void OnEnable()
    {
        DraggableObjectEvents.OnDragStateChanged += HandleDragStateChanged;
        DraggableObjectEvents.OnValidStateChanged += HandleValidStateChanged;
    }

    private void OnDisable()
    {
        DraggableObjectEvents.OnDragStateChanged -= HandleDragStateChanged;
        DraggableObjectEvents.OnValidStateChanged -= HandleValidStateChanged;
    }

    private void HandleDragStateChanged(DraggableObject obj, bool isDragging)
    {
        if (isDragging)
        {
            currentDraggingObject = obj;
            FreezeAllObjects();
        }
        else
        {
            currentDraggingObject = null;
            UnfreezeAllObjects();
        }
    }

    private void HandleValidStateChanged(DraggableObject obj, bool isValid)
    {
        hasInvalidPlacement = !isValid;
    }
} 