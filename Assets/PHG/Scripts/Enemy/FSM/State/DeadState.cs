using System.Collections;
using UnityEngine;
using LHE;

namespace PHG
{
    public class DeadState : IState
    {
        private readonly MonsterBrain brain;

        public DeadState(MonsterBrain brain)
        {
            this.brain = brain;
        }

        public void Enter()
        {
            Debug.Log($"[DeadState] {brain.name} ���� ����");
            // 1. ���� ���
            var player = GameObject.FindWithTag("Player");
            Debug.Log("[DeadState] ã�� �÷��̾� ������Ʈ �̸�: " + player?.name);
            if (player != null && brain.StatData != null)
            {
                var rewards = player.GetComponent<LHE.PlayerRewards>();
                var stats = player.GetComponent<SCR.PlayerStats>();
                Debug.Log("[DeadState] PlayerStats ���� ����: " + (stats != null));
                Debug.Log("[DeadState] PlayerRewards ���� ����: " + (rewards != null));
                if (rewards != null && stats != null)
                {
                    float T = DangerIndexManager.Instance.GetDangerIndex();
                    float S = brain.SpawnStage;
                    float coeff = (1.0f + 0.1012f * T) * Mathf.Pow(1.15f, S);
                    float rewardCoeff = brain.StatData.rewardCoefficient;
                    float goldCoeff = 1.0f;

                    //int exp = coeff * rewardCoeff > 1f ? (int)(coeff * rewardCoeff) : 1; <-//
                    int exp = Mathf.RoundToInt(coeff * brain.StatData.expReward); // ���� ������
                    int gold = Mathf.RoundToInt(coeff * brain.StatData.goldReward * rewardCoeff * goldCoeff);
                    Debug.Log($"���� ���: ����ġ {exp}, ��� {gold}");
                    stats.CurrentExp += exp; // ����ġ �߰�
                    stats.Money += gold; // ��� �߰�
                }
                else
                {
                    Debug.LogWarning($"[DeadState] PlayerRewards �Ǵ� PlayerStats ������Ʈ�� �����Ǿ����ϴ�.");
                }
            }
            else
            {
                Debug.LogWarning($"[DeadState] Player ������Ʈ�� ã�� �� ���ų�, MonsterStats�� �����Ǿ����ϴ�.");
            }

            // 2. ����
            brain.GetComponent<LootDropper>()?.Drop();

            // 3. ��� �ִϸ��̼� + ��Ȱ��ȭ ó��
            brain.StartCoroutine(DieCoroutine());
        }

        public void Tick() { }

        public void Exit() { }

        private IEnumerator DieCoroutine()
        {
            var anim = brain.GetComponent<Animator>();

            if (brain.StatData.hasIdleAnim)
                brain.PlayAnim(AnimNames.Dead);


            yield return new WaitForSeconds(1.5f); // ��� ���� ���

            brain.gameObject.SetActive(false); // ������Ʈ Ǯ ��ȯ
        }
    }
}