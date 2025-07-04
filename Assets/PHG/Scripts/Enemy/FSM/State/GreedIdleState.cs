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
        public void Tick() { /* 대기 */ }

        void HandleInteract()
        {
            interact.OnInteract -= HandleInteract;
            brain.StartCoroutine(PlayOpenThenChase());
           
        }
        IEnumerator PlayOpenThenChase()
        {
            anim.Play("Open", 0, 0f);   // 0 프레임부터 재생
            yield return null;          // 한 프레임 기다려 Animator 갱신

            // 'Open' 클립이 끝날 때까지 대기
            while (anim.GetCurrentAnimatorStateInfo(0).IsName("Open") &&
                   anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                yield return null;

            brain.ChangeState(StateID.Chase);
        }
    }

}