using UnityEngine;

public abstract class UnlockConditionChecker : MonoBehaviour
{
    protected CollectibleObject collectible;
    
    protected virtual void Start()
    {
        collectible = GetComponent<CollectibleObject>();
    }
    
    public abstract bool CheckUnlockCondition();
}