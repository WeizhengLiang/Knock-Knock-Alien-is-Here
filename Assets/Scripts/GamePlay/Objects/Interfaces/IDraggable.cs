using UnityEngine;

public interface IDraggable
{
    bool IsDragging { get; }
    void StartDrag(Vector2 position);
    void UpdateDrag(Vector2 position);
    void EndDrag();
}
