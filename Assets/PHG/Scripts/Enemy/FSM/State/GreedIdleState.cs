using System.Collections;
using UnityEngine;

namespace PHG
{
    public class GreedIdleState : IState
    {
        readonly MonsterBrain brain;
        readonly Interactable interact;
        readonly Animator anim;

        public GreedIdleState(MonsterBrain b, Interactable it)
        {
            brain = b;
            interact = it;
            anim = b.GetComponent<Animator>(); 
        }
        public void Enter() => interact.OnInteract += HandleInteract;
        public void Exit() => interact.OnInteract -= HandleInteract;
        public void Tick() { /* ��� */ }

        void HandleInteract()
        {
            interact.OnInteract -= HandleInteract;
            brain.StartCoroutine(PlayOpenThenChase());
           
        }
        IEnumerator PlayOpenThenChase()
        {
            anim.Play("Open", 0, 0f);   // 0 �����Ӻ��� ���
            yield return null;          // �� ������ ��ٷ� Animator ����

            // 'Open' Ŭ���� ���� ������ ���
            while (anim.GetCurrentAnimatorStateInfo(0).IsName("Open") &&
                   anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                yield return null;

            brain.ChangeState(StateID.Chase);
        }
    }

}