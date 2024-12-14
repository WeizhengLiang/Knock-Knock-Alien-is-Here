public static class DraggableObjectEvents
{
    public delegate void DragStateChanged(DraggableObject obj, bool isDragging);
    public delegate void ValidStateChanged(DraggableObject obj, bool isValid);
    
    public static event DragStateChanged OnDragStateChanged;
    public static event ValidStateChanged OnValidStateChanged;

    public static void RaiseDragStateChanged(DraggableObject obj, bool isDragging)
    {
        OnDragStateChanged?.Invoke(obj, isDragging);
    }

    public static void RaiseValidStateChanged(DraggableObject obj, bool isValid)
    {
        OnValidStateChanged?.Invoke(obj, isValid);
    }
} 