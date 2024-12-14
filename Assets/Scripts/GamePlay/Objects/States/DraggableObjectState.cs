using UnityEngine;

public class DraggableObjectState
{
    private bool isDragging;
    private bool isInvalidPosition;
    private bool isMouseOver;
    private bool isFrozen;
    private readonly DraggableObject owner;

    public bool IsDragging => isDragging;
    public bool IsInvalidPosition => isInvalidPosition;
    public bool IsMouseOver => isMouseOver;
    public bool IsFrozen => isFrozen;

    public DraggableObjectState(DraggableObject owner)
    {
        this.owner = owner;
    }

    public void SetDragging(bool value)
    {
        if (isDragging != value)
        {
            isDragging = value;
            DraggableObjectManager.Instance?.SetDragging(value, owner);
            DraggableObjectEvents.RaiseDragStateChanged(owner, value);
        }
    }

    public void SetInvalidPosition(bool value)
    {
        if (isInvalidPosition != value)
        {
            isInvalidPosition = value;
            DraggableObjectManager.Instance?.SetInvalidPlacement(value);
            DraggableObjectEvents.RaiseValidStateChanged(owner, !value);
        }
    }

    public void SetMouseOver(bool value)
    {
        isMouseOver = value;
    }

    public void SetFrozen(bool value)
    {
        isFrozen = value;
    }
}