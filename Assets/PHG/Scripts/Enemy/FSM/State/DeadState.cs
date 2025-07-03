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
            Debug.Log($"[DeadState] {brain.name} 상태 진입");
            // 1. 보상 계산
            var player = GameObject.FindWithTag("Player");
            Debug.Log("[DeadState] 찾은 플레이어 오브젝트 이름: " + player?.name);
            if (player != null && brain.StatData != null)
            {
                var rewards = player.GetComponent<LHE.PlayerRewards>();
                var stats = player.GetComponent<SCR.PlayerStats>();
                Debug.Log("[DeadState] PlayerStats 존재 여부: " + (stats != null));
                Debug.Log("[DeadState] PlayerRewards 존재 여부: " + (rewards != null));
                if (rewards != null && stats != null)
                {
                    float T = DangerIndexManager.Instance.GetDangerIndex();
                    float S = brain.SpawnStage;
                    float coeff = (1.0f + 0.1012f * T) * Mathf.Pow(1.15f, S);
                    float rewardCoeff = brain.StatData.rewardCoefficient;
                    float goldCoeff = 1.0f;

                    //int exp = coeff * rewardCoeff > 1f ? (int)(coeff * rewardCoeff) : 1; <-//
                    int exp = Mathf.RoundToInt(coeff * brain.StatData.expReward); // 현재 디버깅용
                    int gold = Mathf.RoundToInt(coeff * brain.StatData.goldReward * rewardCoeff * goldCoeff);
                    Debug.Log($"보상 계산: 경험치 {exp}, 골드 {gold}");
                    stats.CurrentExp += exp; // 경험치 추가
                    stats.Money += gold; // 골드 추가
                }
                else
                {
                    Debug.LogWarning($"[DeadState] PlayerRewards 또는 PlayerStats 컴포넌트가 누락되었습니다.");
                }
            }
            else
            {
                Debug.LogWarning($"[DeadState] Player 오브젝트를 찾을 수 없거나, MonsterStats가 누락되었습니다.");
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

            if (brain.StatData.hasIdleAnim)
                brain.PlayAnim(AnimNames.Dead);


            yield return new WaitForSeconds(1.5f); // 사망 연출 대기

            brain.gameObject.SetActive(false); // 오브젝트 풀 반환
        }
    }
}