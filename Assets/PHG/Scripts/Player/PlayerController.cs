// PlayerController2D.cs
using UnityEngine;

namespace PHG
{
    /// <summary>
    /// �⺻ �̵������� ��Ʈ�ѷ�
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
    public class PlayerController2D : MonoBehaviour
    {
        /* �������������������������������� Member ���� �������������������������������� */
        [SerializeField] private float moveSpeed = 5f;          // ��� �̵� �ӵ�
        [SerializeField] private float jumpForce = 12f;         // ���� ��
        [SerializeField] private LayerMask groundMask;          // ���� ���̾�
        [SerializeField] private Transform groundCheck;         // �� �Ʒ� üũ ��ġ
        [SerializeField] private float groundCheckRadius = 0.15f;

        /* �������������������������������� Private �������������������������������� */
        private Rigidbody2D rb;
        private bool isGrounded;

        /* �������������������������������� Unity Lifecycle �������������������������������� */
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.freezeRotation = true;               // 2D ��� ����
        }

        private void Update()
        {
            Move(Input.GetAxisRaw("Horizontal"));
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded) Jump();
        }

        private void FixedUpdate() => CheckGround();

        /* �������������������������������� Core �������������������������������� */
        private void Move(float dir /* �Ű����� */)
        {
            rb.velocity = new Vector2(dir * moveSpeed, rb.velocity.y);
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(dir);
            transform.localScale = scale;
        }

        private void Jump()
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);        // ���� �� Y�ӵ� ����
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        private void CheckGround()
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
        }

#if UNITY_EDITOR
        // �����Ϳ��� GroundCheck �ð�ȭ
        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null) return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
#endif
    }
}