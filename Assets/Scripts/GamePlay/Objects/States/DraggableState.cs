public class DraggableState
{
    public event System.Action<bool> OnDragStateChanged;
    public event System.Action<bool> OnValidStateChanged;

    private bool isDragging;
    private bool isValidPosition = true;

    public bool IsDragging => isDragging;
    public bool IsValidPosition => isValidPosition;

    public void SetDragging(bool value)
    {
        if (isDragging != value)
        {
            isDragging = value;
            OnDragStateChanged?.Invoke(value);
        }
    }

    public void SetValidPosition(bool value)
    {
        if (isValidPosition != value)
        {
            isValidPosition = value;
            OnValidStateChanged?.Invoke(value);
        }
    }
} 