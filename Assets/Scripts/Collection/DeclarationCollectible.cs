// 星际友好宣言（摔落解锁）
public class DeclarationCollectible : CollectibleObject
{
    private DropUnlockChecker dropChecker;
    
    protected override void Start()
    {
        base.Start();
        dropChecker = GetComponent<DropUnlockChecker>();
    }
    
    protected override void Update()
    {
        base.Update();
        if (!isUnlocked && dropChecker != null && dropChecker.CheckUnlockCondition())
        {
            Unlock();
        }
    }
}