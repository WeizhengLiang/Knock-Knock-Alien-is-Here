using UnityEngine;

public class PlacementSystem : IGameSystem
{
    private readonly DraggableObject owner;
    private readonly Collider2D col;
    private readonly LayerMask collisionLayer;
    private readonly float surfaceIgnoreDistance;

    public PlacementSystem(DraggableObject owner, LayerMask collisionLayer, float surfaceIgnoreDistance)
    {
        this.owner = owner;
        this.col = owner.GetComponent<Collider2D>();
        this.collisionLayer = collisionLayer;
        this.surfaceIgnoreDistance = surfaceIgnoreDistance;
    }

    public void Initialize() { }

    public void Update()
    {
        if (owner.IsDragging)
        {
            ValidatePlacement();
        }
    }

    public void FixedUpdate() { }

    public virtual bool ValidatePlacement()
    {
        if (col == null) return true;

        Collider2D[] colliders = Physics2D.OverlapBoxAll(
            owner.transform.position,
            col.bounds.size,
            owner.transform.rotation.eulerAngles.z,
            collisionLayer
        );

        bool isValid = true;
        foreach (Collider2D otherCol in colliders)
        {
            if (otherCol != col && otherCol.gameObject != owner.gameObject)
            {
                ColliderDistance2D distance = Physics2D.Distance(col, otherCol);
                if (distance.distance < -surfaceIgnoreDistance)
                {
                    isValid = false;
                    break;
                }
            }
        }

        owner.state.SetInvalidPosition(!isValid);
        return isValid;
    }

    public void OnDrawGizmos()
    {
        if (col == null) return;

        // 绘制碰撞范围
        Gizmos.color = owner.IsInvalidPosition ? Color.red : Color.blue;
        DrawColliderBounds(col);

        // 绘制表面忽略距离
        DrawSurfaceIgnoreDistance();
    }

    private void DrawColliderBounds(Collider2D collider)
    {
        if (collider is PolygonCollider2D poly)
        {
            Vector2[] points = poly.points;
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 current = owner.transform.TransformPoint(points[i]);
                Vector2 next = owner.transform.TransformPoint(points[(i + 1) % points.Length]);
                Gizmos.DrawLine(current, next);
            }
        }
    }

    private void DrawSurfaceIgnoreDistance()
    {
        if (!(col is PolygonCollider2D poly)) return;

        Vector2[] points = poly.points;
        for (int i = 0; i < points.Length; i++)
        {
            Vector2 current = owner.transform.TransformPoint(points[i]);
            Vector2 next = owner.transform.TransformPoint(points[(i + 1) % points.Length]);
            Vector2 dir = (next - current).normalized;
            Vector2 normal = new Vector2(-dir.y, dir.x);
            
            Gizmos.DrawLine(
                current + normal * surfaceIgnoreDistance, 
                next + normal * surfaceIgnoreDistance
            );
        }
    }
} 