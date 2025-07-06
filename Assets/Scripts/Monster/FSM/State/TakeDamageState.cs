using UnityEngine;


/// <summary>
/// 몬스터가 피격되었을 때 실행되는 FSM 상태입니다.
/// 일정 시간 동안 경직/넉백 및 UI 출력 후 이전 상태로 복귀합니다.
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
        Debug.Log($"현재 hp :{_monsterStats.CurrentHP}");

        // 체력 감소
        int newHP = _monsterStats.CurrentHP - _damage;
        _monsterStats.SetHP(newHP);

        //Debug.Log($"[TakeDamageState] CurrentHP: {stats.CurrentHP}, MaxHP: {stats.MaxHP}");

        // 체력바 갱신
        if (_monster.HpBarFill != null)
        {
            _monster.HpBar.gameObject.SetActive(true);
            _monster.HpBarFill.fillAmount = (float)_monsterStats.CurrentHP / _monsterStats.MaxHP;
        }

        // 데미지 텍스트 출력
        //  brain.ShowDamageText(hit.damage);

        // 사망 처리
        _monsterStats.KillIfDead();
        if (_monsterStats.CurrentHP <= 0)
            return;

        // 넉백
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
/// 피격 정보를 담는 구조체. 데미지, 넉백 방향, 경직 여부 등 포함
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