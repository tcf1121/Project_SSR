using System.Collections;
using UnityEngine;
using Utill;


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
        GameManager.Player.GetReward(brain.Monster.MonsterStats.Gold, brain.Monster.MonsterStats.Exp);

        // 2. ��� �ִϸ��̼� + ��Ȱ��ȭ ó��
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
        if (brain.IsFlying) ObjectPool.ReturnPool(brain.gameObject, EPoolObjectType.FlyMonster);
        else if (brain.IsRanged) ObjectPool.ReturnPool(brain.gameObject, EPoolObjectType.LDMonster);
        else ObjectPool.ReturnPool(brain.gameObject, EPoolObjectType.CDMonster);

    }
}