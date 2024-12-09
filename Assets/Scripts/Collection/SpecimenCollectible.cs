using UnityEngine;

public class SpecimenCollectible : CollectibleObject
{
    [Header("Break Settings")]
    [SerializeField] public float breakForce = 8f;
    [SerializeField] private GameObject brokenPrefab;         
    [SerializeField] private GameObject groundItemPrefab;     
    [SerializeField] private float groundY = 0f;             
    [SerializeField] private ParticleSystem glassBreakEffect;

    private float accumulatedForce = 0f;                     
    private float lastForceTime;                             
    [SerializeField] private float breakForceThreshold = 5f; 
    [SerializeField] private float forceResetTime = 0.5f;    

    protected override void Start()
    {
        base.Start();
        lastForceTime = Time.time;

        if (IsUnlocked())
        {
            if (unlockVisualEffect != null)
            {
                unlockVisualEffect.SetActive(true);
            }
            isUnlocked = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isUnlocked || isDragging) return;

        Vector2 relativePoint = collision.GetContact(0).point - (Vector2)transform.position;
        bool isTopCollision = relativePoint.y > 0;

        float impactForce = collision.GetContact(0).normalImpulse;
        if (isTopCollision)
        {
            impactForce *= (collision.rigidbody?.mass ?? 1f);
        }

        if (impactForce > breakForce)
        {
            Break(collision.GetContact(0).point);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isUnlocked || isDragging) return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.point.y > transform.position.y &&
                collision.gameObject.transform.position.y > transform.position.y)
            {
                float appliedForce = contact.normalImpulse;

                float currentTime = Time.time;
                if (currentTime - lastForceTime > forceResetTime)
                {
                    accumulatedForce = 0f;
                }

                accumulatedForce += appliedForce * Time.deltaTime;
                lastForceTime = currentTime;

                if (accumulatedForce >= breakForceThreshold)
                {
                    Break(contact.point);
                }
                break;
            }
        }
    }

    private void Break(Vector2 breakPoint)
    {
        if (glassBreakEffect != null)
        {
            glassBreakEffect.Play();
        }

        if (brokenPrefab != null)
        {
            GameObject broken = Instantiate(brokenPrefab, transform.position, transform.rotation);
            
            if (rb != null && broken.TryGetComponent<Rigidbody2D>(out var brokenRb))
            {
                brokenRb.velocity = rb.velocity;
                brokenRb.angularVelocity = rb.angularVelocity;
            }
        }

        if (groundItemPrefab != null)
        {
            Vector3 groundPosition = new Vector3(breakPoint.x, groundY, 0);
            Instantiate(groundItemPrefab, groundPosition, Quaternion.identity);
            SoundManager.Instance.PlaySoundFromResources("Sound/Runaway", "Runaway", false, 1.0f);
        }

        if (!isUnlocked)
        {
            Unlock();
        }

        Destroy(gameObject);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.transform.position.y > transform.position.y)
        {
            accumulatedForce = 0f;
            lastForceTime = Time.time;
        }
    }

    private void OnDrawGizmos()
    {
        if (!isUnlocked && Application.isPlaying && accumulatedForce > 0)
        {
            float radius = accumulatedForce / breakForceThreshold * 0.5f;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}