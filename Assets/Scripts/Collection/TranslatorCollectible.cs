using UnityEngine;

public class TranslatorCollectible : CollectibleObject
{
    [SerializeField] private LayerMask diskLayer;
    [SerializeField] private float checkRadius = 0.5f;
    [SerializeField] private Transform diskSlot;
    [SerializeField] private ParticleSystem processingEffect;
    
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
        Collider2D[] colliders = Physics2D.OverlapCircleAll(diskSlot.position, checkRadius, diskLayer);
        if (colliders.Length > 0)
        {
            isDiskInserted = true;
            if (processingEffect != null)
            {
                processingEffect.Play();
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