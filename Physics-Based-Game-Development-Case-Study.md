# Physics-Based Game Development: Typical Problems and Solutions  
**Case Study: "Knock Knock Alien is Here" (Unity Physics API)**

---

## Table of Contents
- [Potential Problems](#potential-problems)  
- [How this project solves the popping problem?](#how-this-project-solves-the-popping-problem)  
  - [The Tolerance System](the-tolerance-system)  
- [How this project solves SetPosition problem?](#how-this-project-solves-setposition-problem)  
  - [Complete Flow](#complete-flow)  
  - [There are 3 layers in the above complete flow](#there-are-3-layers-in-the-above-complete-flow)  
    - [Layer 1: Prevent Direct Position Setting](#layer-1-prevent-direct-position-setting)  
    - [Layer 2: Controlled Overlap Tolerance](#layer-2-controlled-overlap-tolerance)  
    - [Layer 3: Velocity-Based Placement](#layer-3-velocity-based-placement)  
- [In short conclusion](#in-short-conclusion)  

---

There are some typical problems we would encounter during phsics-based game development.  
I would like to address two of them in this documentation, specifically for scenario such as in this game "Knock Knock Alien is Here". Illustrated with Unity's phsics API.

---

## Potential Problems

1. Physics conflicts when setting positions directly  
2. Objects "popping" or "jumping" when physics tries to resolve overlaps

In Unity engine, this can be interpret as the following:  
An object suddenly teleported to a new position.  
If there is an overlap then the system must resolve it immediately.  
The system then apply separation forces to push objects apart which result in objects "pop" and jitter.

---

## How this project solves the popping problem?

### The Tolerance System

```csharp
//GamePlay/Objects/DraggableObject.cs

[SerializeField] protected float surfaceIgnoreDistance = 0.3f; // 0.3 units tolerance

protected virtual bool IsValidPlacement()
{
    Collider2D[] colliders = Physics2D.OverlapBoxAll(/*...*/);
    
    foreach (Collider2D otherCol in colliders)
    {
        // Get precise distance between colliders
        ColliderDistance2D distance = Physics2D.Distance(col, otherCol);
        
        // Only invalid if overlap exceeds tolerance
        if (distance.distance < -surfaceIgnoreDistance) // -0.3f
        {
            isValid = false; // Invalid placement
            break;
        }
    }
    return isValid;
}
```

### By allowing controlled overlap, the system prevents unnecessary separation forces:

```csharp
// Without tolerance (causes popping):
if (objectsOverlap) {
    // Unity: "Objects are overlapping! Must separate immediately!"
    // Result: Objects "pop" apart with sudden forces
}

// With tolerance (prevents popping):
if (overlap > tolerance) {
    // Only separate if overlap is significant
    // Small overlaps are allowed, no forces applied
    // Result: Smooth, natural behavior
}
```

## How this project solves SetPosition problem?

### Complete Flow

```csharp
//GamePlay/Objects/DraggableObject.cs

// 1. During drag - use joint system (no direct position setting)
private void UpdateDragPosition() 
{
    // Move the anchor, not the object directly
    Vector2 newPosition = Vector2.Lerp(currentAnchorPos, mousePos, dragSpeed * Time.deltaTime);
    dragAnchor.transform.position = newPosition; // Move anchor, not object
}

// 2. Placement validation - check overlap tolerance
protected virtual bool IsValidPlacement()
{
    // Only invalid if overlap exceeds tolerance
    if (distance.distance < -surfaceIgnoreDistance) {
        return false; // Prevent placement
    }
    return true; // Allow placement
}

// 3. On placement - use velocity instead of direct positioning
protected virtual void CompletePlace()
{
    // Apply controlled velocity instead of direct position setting
    Vector2 finalVelocity = dragVelocity / Time.fixedDeltaTime;
    rb.velocity = finalVelocity * dragInertiaMultiplier; // 0.3f multiplier
    
    // Let Unity's physics handle the rest naturally
}
```
### There are 3 layers in the above complete flow:

#### Layer 1: Prevent Direct Position Setting
```csharp
// Use joint system instead of transform.position
dragJoint = gameObject.AddComponent<HingeJoint2D>();
// Object follows anchor smoothly, no sudden position changes
```

#### Layer 2: Controlled Overlap Tolerance
```csharp
// Allow slight overlaps to prevent unnecessary separation
if (distance.distance < -surfaceIgnoreDistance) {
    // Only reject if overlap is significant
}
```

#### Layer 3: Velocity-Based Placement
```csharp
// Use physics velocity instead of direct positioning
rb.velocity = finalVelocity * dragInertiaMultiplier;
// Let Unity's physics handle the final positioning
```

## In short conclusion

The controlled overlap tolerance prevents the physics engine from overreacting to minor overlaps.
In addition, the hingeJoint component helps object reserves a natural landing and collision.
With these two in hand, even under extrem circumstance like low fps rate, it can still create some space for user experience.



