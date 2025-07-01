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
            // 1. ���� ���
            var player = GameObject.FindWithTag("Player");
            if (player != null && brain.statData != null)
            {
                var rewards = player.GetComponent<PlayerRewards>();
                if (rewards != null)
                {
                    float T = DangerIndexManager.Instance.GetDangerIndex();
                    float S = brain.SpawnStage;
                    float coeff = (1.0f + 0.1012f * T) * Mathf.Pow(1.15f, S);
                    float rewardCoeff = brain.statData.rewardCoefficient;
                    float goldCoeff = 1.0f;

                    int exp = coeff * rewardCoeff > 1f ? (int)(coeff * rewardCoeff) : 1;
                    int gold = coeff * rewardCoeff * goldCoeff > 1f ? (int)(coeff * rewardCoeff * goldCoeff) : 1;

                    rewards.GetCompensation(gold, exp);
                }
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
            if (anim != null)
                anim.SetTrigger("Die");

            yield return new WaitForSeconds(1.5f); // ��� ���� ���

            brain.gameObject.SetActive(false); // ������Ʈ Ǯ ��ȯ
        }
    }
}