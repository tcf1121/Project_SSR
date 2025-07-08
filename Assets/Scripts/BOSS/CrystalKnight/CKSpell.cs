using UnityEngine;

public class CKSpell : MonoBehaviour
{
    public float damage;
    public float speed = 10f;
    public float lifetime = 5f;

    private Rigidbody2D rb;
    private Vector2 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            rb.velocity = moveDirection * speed;
        }
    }

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall")) 
        { 
            Destroy(gameObject);
        }
    }
}
