using UnityEngine;


/// <summary>
/// ���Ͱ� �ǰݵǾ��� �� ����Ǵ� FSM �����Դϴ�.
/// ���� �ð� ���� ����/�˹� �� UI ��� �� ���� ���·� �����մϴ�.
/// </summary>
public class TakeDamageState : IState
{
    private readonly Monster _monster;
    private readonly MonsterStats _monsterStats;
    private float timer;
    private int _damage;
    private bool stagger;

    public TakeDamageState(Monster monster, int damage)
    {
        _monster = monster;
        _monsterStats = _monster.MonsterStats;
        _damage = damage;
    }

    public void SetDamage(int damage)
    {
        _damage = damage;
    }

    public void Enter()
    {
        Debug.Log($"���� hp :{_monsterStats.CurrentHP}");

        // ü�� ����
        int newHP = _monsterStats.CurrentHP - _damage;
        _monsterStats.SetHP(newHP);

        //Debug.Log($"[TakeDamageState] CurrentHP: {stats.CurrentHP}, MaxHP: {stats.MaxHP}");

        // ü�¹� ����
        if (_monster.HpBarFill != null)
        {
            _monster.HpBar.gameObject.SetActive(true);
            _monster.HpBarFill.fillAmount = (float)_monsterStats.CurrentHP / _monsterStats.MaxHP;
        }

        // ������ �ؽ�Ʈ ���
        //  brain.ShowDamageText(hit.damage);

        // ��� ó��
        _monsterStats.KillIfDead();
        if (_monsterStats.CurrentHP <= 0)
            return;

        // �˹�
        _monster.KnockBack();
        timer = 0.35f;

    }

    public void Tick()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            _monster.ChangeState(StateID.Idle);
        }
    }

    public void Exit()
    {
        _monster.Rigid.velocity = Vector2.zero;
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