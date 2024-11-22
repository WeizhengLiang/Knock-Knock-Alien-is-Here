using UnityEngine;

public class ExplosionForce : MonoBehaviour
{
    public float explosionForceMax = 10f;
    public float explosionForceMin = 2f;


    void Start()
    {
        Explode();
    }

    void Explode()
    {
        foreach (Transform child in transform)
        {
            Rigidbody2D rb = child.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                Vector2 direction = child.position - transform.position;
                direction.Normalize();
                float explosionForce = Random.Range(explosionForceMin, explosionForceMax);
                rb.AddForce(direction * explosionForce, ForceMode2D.Impulse);

            }
        }
    }
}
