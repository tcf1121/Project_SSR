using UnityEngine;

namespace PHG
{
    /// <summary>
    /// ��� �պ����� �����ϴٰ� �÷��̾ ��������� ���� ���·� ��ȯ
    /// </summary>
    public class PatrolState : IState
    {
        /* ���������� refs ���������� */
        private readonly MonsterBrain brain;
        private readonly Rigidbody2D rb;
        private readonly Transform tf;
        private readonly Transform sensor;
        private readonly LayerMask groundMask;
        private readonly MonsterStatData statData;

        /* ���������� patrol vars ���������� */
        private int dir = 1;                       // 1=��, -1=��
        private const float floorCheckDist = 0.4f; // �߾Ʒ� Ȯ��
        private const float wallCheckDist = 0.2f; // �� Ȯ�� �Ÿ�

        public PatrolState(MonsterBrain brain)
        {
            this.brain = brain;
            this.rb = brain.GetComponent<Rigidbody2D>();
            this.tf = brain.transform;
            this.sensor = brain.sensor;
            this.groundMask = brain.groundMask;
            this.statData = brain.StatData;        // ScriptableObject ��ġ ���
        }

        /*���������������� IState ���� ����������������*/
        public void Enter() => dir = tf.localScale.x >= 0f ? 1 : -1;

        public void Tick()
        {
            if (statData == null) return; // ������

            /* 1) �̵� ---------------------------------- */
            rb.velocity = new Vector2(dir * statData.moveSpeed, rb.velocity.y);

            /* 2) ������������ ���� �� ���� ���� */
            Vector2 groundCheckPos = sensor.position + Vector3.right * dir * 0.3f + Vector3.down * 0.1f;
            float groundCheckRadius = 0.1f;

            bool noFloor = !Physics2D.OverlapCircle(groundCheckPos, groundCheckRadius, groundMask);
            bool hitWall = Physics2D.Raycast(sensor.position, Vector2.right * dir, wallCheckDist, groundMask);

            if (noFloor || hitWall)
            {
                dir *= -1;
                Vector3 scale = tf.localScale;
                scale.x = Mathf.Abs(scale.x) * dir;
                tf.localScale = scale;
            }

            /* 3) ���� �Ǵ� ���� ��ȯ */
            if (PlayerInRange(statData.attackRange))
            {
                rb.velocity = Vector2.zero;
                brain.ChangeState(StateID.Attack);
                return;
            }
            if (PlayerInRange(statData.patrolRange))
            {
                brain.ChangeState(StateID.Chase);
                return;
            }

            if (PlayerInRange(statData.readyRange))   // ���� ��� ����
            {
                FacePlayer();                         // ���⸸ ���߰�
                rb.velocity = Vector2.zero;           // �̵� ����
                brain.ChangeState(StateID.Attack);    // �� RangeAttackState
                return;
            }

        }
    

        public void Exit() => rb.velocity = Vector2.zero;

        /*���������������� ���� ����������������*/
        private bool PlayerInRange(float range)
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null) return false;
            return Vector2.Distance(tf.position, player.transform.position) <= range;
        }

        void FacePlayer()
        {
            var pl = GameObject.FindWithTag("Player")?.transform;
            if (pl == null) return;

            int sign = pl.position.x > tf.position.x ? 1 : -1;
            Vector3 s = tf.localScale;
            s.x = Mathf.Abs(s.x) * sign;
            tf.localScale = s;
        }
    }
}
