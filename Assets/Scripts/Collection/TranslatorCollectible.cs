using UnityEngine;

public class TranslatorCollectible : CollectibleObject
{
    [SerializeField] private float checkRadius = 0.5f;
    [SerializeField] private Transform diskSlot;
    
    private bool isDiskInserted = false;
    private float processingTime = 0f;
    private const float REQUIRED_PROCESSING_TIME = 3f;
    
    protected override void Update()
    {
        base.Update();
        if (!isUnlocked)
        {
            if (!isDiskInserted)
            {
                CheckDiskInsertion();
            }
            else
            {
                ProcessDisk();
            }
        }
    }
    
    private void CheckDiskInsertion()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(diskSlot.position, checkRadius);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Disk"))
            {
                isDiskInserted = true;
                break;
            }
        }
    }
    
    private void ProcessDisk()
    {
        processingTime += Time.deltaTime;
        if (processingTime >= REQUIRED_PROCESSING_TIME)
        {
            Unlock();
        }
    }
}