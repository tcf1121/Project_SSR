using UnityEngine;
using UnityEngine.UI;

namespace PHG
{
    /// <summary>
    /// ���Ͱ� �ǰݵǾ��� �� ����Ǵ� FSM �����Դϴ�.
    /// ���� �ð� ���� ����/�˹� �� UI ��� �� ���� ���·� �����մϴ�.
    /// </summary>
    public class TakeDamageState : IState
    {
        private readonly MonsterBrain brain;
        private readonly Rigidbody2D rb;
        private readonly HitInfo hit;

        private float timer;
        private bool stagger;

        public TakeDamageState(MonsterBrain brain, HitInfo hit)
        {
            this.brain = brain;
            this.rb = brain.rb;
            this.hit = hit;
        }

        public void Enter()
        {
            Debug.Log("[TakeDamageState] Enter");

            var stats = brain.RuntimeStats;
           // brain.ShowHpBarTemporarily();

            // ü�� ����
            int newHP = stats.CurrentHP - hit.damage;
            stats.SetHP(newHP);

            Debug.Log($"[TakeDamageState] CurrentHP: {stats.CurrentHP}, MaxHP: {stats.MaxHP}");

            // ü�¹� ����
            if (brain.hpBarRoot != null)
            {
                var slider = brain.hpBarRoot.GetComponentInChildren<Slider>();
                if (slider != null)
                    slider.value = (float)stats.CurrentHP / stats.MaxHP;
            }

            // ������ �ؽ�Ʈ ���
          //  brain.ShowDamageText(hit.damage);

            // ��� ó��
            stats.KillIfDead();
            if (stats.CurrentHP <= 0)
                return;

            // ���� ���� �Ǵ�
            stagger = hit.causesStagger || hit.damage >= brain.statData.staggerThreshold;

            if (stagger)
            {
                float knockback = brain.statData.knockbackForce; // �� SO���� ����
                brain.ApplyKnockback(hit.origin, knockback);
                timer = 0.35f;
            }
            else
            {
                timer = 0.1f;
            }
        }

        public void Tick()
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                brain.Sm.ChangeState(StateID.Idle);
            }
        }

        public void Exit()
        {
            rb.velocity = Vector2.zero;
        }
    }

    /// <summary>
    /// �ǰ� ������ ��� ����ü. ������, �˹� ����, ���� ���� �� ����
    /// </summary>
    public struct HitInfo
    {
        public int damage;
        public Vector2 origin;
        public bool causesStagger;

        public HitInfo(int dmg, Vector2 origin, bool stagger = false)
        {
            this.damage = dmg;
            this.origin = origin;
            this.causesStagger = stagger;
        }
    }
}