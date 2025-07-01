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
            // 1. 보상 계산
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

            // 2. 루팅
            brain.GetComponent<LootDropper>()?.Drop();

            // 3. 사망 애니메이션 + 비활성화 처리
            brain.StartCoroutine(DieCoroutine());
        }

        public void Tick() { }

        public void Exit() { }

        private IEnumerator DieCoroutine()
        {
            var anim = brain.GetComponent<Animator>();
            if (anim != null)
                anim.SetTrigger("Die");

            yield return new WaitForSeconds(1.5f); // 사망 연출 대기

            brain.gameObject.SetActive(false); // 오브젝트 풀 반환
        }
    }
}