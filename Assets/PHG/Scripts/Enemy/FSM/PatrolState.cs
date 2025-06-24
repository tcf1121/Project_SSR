using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PHG;
/// <summary>
/// ��� �պ����� �����ϴٰ� �÷��̾ ��������� ���� ���·� ��ȯ
/// </summary>
namespace PHG
{
    public class PatrolState : IState
    {
        private readonly MonsterBrain brain;
        private readonly Rigidbody2D rb;
        private readonly Transform tf;
        private readonly Transform sensor;
        private readonly LayerMask groundMask;

        private int dir = 1; // 1= -> , -1 = <-
        private const float floorCheckDist = 0.2f; //�߾Ʒ� Ȯ��
        private const float wallCheckDist = 0.1f; //�� Ȯ�� �Ÿ�

        public PatrolState(MonsterBrain brain)
        {
            this.brain = brain;
            rb = brain.GetComponent<Rigidbody2D>(); //  ���� rigidbody ĳ��
            tf = brain.transform;

            // MonsterBrain �ν����Ϳ� Sensor, GrounMask ����
            sensor = brain.sensor;
            groundMask = brain.groundMask;
        }
        /*========= IState �������̽� ���� =========*/
        public void Enter() { }   // ó�� ���� �� �� �� ����
        public void Tick()
        {
            /** 1) �̵� �ڵ� ---------------------------------- */
            //  dir(��1) * �̵��ӵ��� X���� �ӵ� �ο�
            rb.velocity = new Vector2(dir * brain.Stats.MoveSpeed, rb.velocity.y);



            /* ��������, ������ -> ���� ���� */
            bool noFloor = !Physics2D.Raycast(sensor.position, Vector2.down, floorCheckDist, groundMask);
            bool hitWall = Physics2D.Raycast(sensor.position, Vector2.right * dir, wallCheckDist, groundMask);

            if (noFloor || hitWall)
            {
                dir *= -1; // ���� ����
                tf.localScale = new Vector3(dir, 1, 1);
                //sensor�� �׻� ���� ���ϵ���
            }
            /* 3) (�ɼ�) ���� ��ȯ */
            if (PlayerInRange(brain.Stats.PatrolRange))
                brain.ChangeState(StateID.Chase);

        }
        // ���¸� ���� �� �̵� ����
        public void Exit() => rb.velocity = Vector2.zero;

        /*�������������������������������� ���� �޼��� ��������������������������������*/
        private bool PlayerInRange(float range)
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null) return false;
            return Vector2.Distance(tf.position, player.transform.position) <= range;
        }


    }
}