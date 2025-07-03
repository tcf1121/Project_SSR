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
            // 1. ���� ���
            GameManager.Player.GetReward(brain.MonsterStats.Gold, brain.MonsterStats.Exp);

            // 2. ��� �ִϸ��̼� + ��Ȱ��ȭ ó��
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
            if (brain.IsFlying) ObjectPool.ReturnPool(brain.gameObject, EPoolObjectType.FlyMonster);
            else if (brain.IsRanged) ObjectPool.ReturnPool(brain.gameObject, EPoolObjectType.LDMonster);
            else ObjectPool.ReturnPool(brain.gameObject, EPoolObjectType.CDMonster);

        }
    }
}