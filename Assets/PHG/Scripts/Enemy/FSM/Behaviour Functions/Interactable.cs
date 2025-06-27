using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PHG
{
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
                Debug.Log("상호작용, 미믹");
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
}