
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    public float lifetime = 1f;
    public float floatSpeed = 0.5f;

    private Vector3 moveDir = Vector3.up;

    void Update()
    {
        transform.position += moveDir * floatSpeed * Time.deltaTime;
        lifetime -= Time.deltaTime;
        if (lifetime <= 0f)
            Destroy(gameObject);
    }
}
