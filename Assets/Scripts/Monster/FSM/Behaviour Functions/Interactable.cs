using UnityEngine;


[RequireComponent(typeof(Collider2D))]
public class Interactable : MonoBehaviour
{
    public event System.Action OnInteract;

    [SerializeField] float interactRadius = 1.2f;
    [SerializeField] LayerMask playerMask;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && PlayerIsClose())
        {
            OnInteract?.Invoke();
        }
    }
    bool PlayerIsClose()
    {
        return Physics2D.OverlapCircle(transform.position,
                                       interactRadius,
                                       playerMask);
    }

    /* gizmo only */
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
#endif
}