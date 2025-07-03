using System.Collections;
using UnityEngine;
using LHE;
using Utill;
using SCR;

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
            GameManager.Player.GetReward(brain.MonsterStats.Gold, brain.MonsterStats.Exp);

            // 2. 사망 애니메이션 + 비활성화 처리
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
            if (brain.IsFlying) ObjectPool.ReturnPool(brain.gameObject, EPoolObjectType.FlyMonster);
            else if (brain.IsRanged) ObjectPool.ReturnPool(brain.gameObject, EPoolObjectType.LDMonster);
            else ObjectPool.ReturnPool(brain.gameObject, EPoolObjectType.CDMonster);

        }
    }
}