using UnityEngine;

public class PhysicsSystem : IGameSystem
{
    private readonly DraggableObject owner;
    private readonly Rigidbody2D rb;
    private readonly Collider2D col;
    
    private Vector2 frozenVelocity;
    private float frozenAngularVelocity;
    private bool wasFrozen;

    public PhysicsSystem(DraggableObject owner)
    {
        this.owner = owner;
        this.rb = owner.GetComponent<Rigidbody2D>();
        this.col = owner.GetComponent<Collider2D>();
        
        InitializePhysics();
    }

    private void InitializePhysics()
    {
        if (rb != null)
        {
            float gravityScale = 1f + (rb.mass - 1f) * 0.5f;
            rb.gravityScale = gravityScale;
        }
        
        if (col != null)
        {
            col.isTrigger = false;
        }
    }

    public void PrepareDrag()
    {
        if (rb == null) return;
        
        rb.isKinematic = false;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    public void Freeze()
    {
        if (rb == null || owner.IsInvalidPosition || owner.IsDragging) return;

        if (!wasFrozen)
        {
            frozenVelocity = rb.velocity;
            frozenAngularVelocity = rb.angularVelocity;

            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true;

            wasFrozen = true;
        }
    }

    public void Unfreeze()
    {
        if (rb == null) return;
        
        if (wasFrozen)
        {
            rb.isKinematic = false;
            rb.velocity = frozenVelocity;
            rb.angularVelocity = frozenAngularVelocity;

            wasFrozen = false;
        }
    }

    public void UpdatePhysicsState()
    {
        if (owner.IsInvalidPosition && !owner.IsDragging)
        {
            StopMovement();
        }
    }

    public void StopMovement()
    {
        if (rb == null) return;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    public void ApplyDragInertia(Vector2 velocity)
    {
        if (rb == null) return;
        rb.velocity = velocity;
    }

    public void SetKinematic(bool isKinematic)
    {
        if (rb == null) return;
        rb.isKinematic = isKinematic;
    }

    public void Initialize()
    {
        InitializePhysics();
    }

    public void Update() { }

    public void FixedUpdate()
    {
        UpdatePhysicsState();
    }
} 