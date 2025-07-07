using UnityEngine;


public class MonsterStats : MonoBehaviour
{

    [SerializeField] private Monster _monster;

    // 런타임 내부 계산 캐시
    [SerializeField] private int _currentHP;
    [SerializeField] private int _maxHP;
    [SerializeField] private int _damage;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _patrolRange;
    [SerializeField] private float _chaseRange;
    [SerializeField] private float _attackRange;
    [SerializeField] private float _chargeRange;
    [SerializeField] private bool _usePatrol;
    [SerializeField] private int _gold;
    [SerializeField] private int _exp;
    [SerializeField] private bool _isDead;

    public int MaxHP { get => _maxHP; }
    public int CurrentHP { get => _currentHP; }
    public int Damage { get => _damage; }
    public float MoveSpeed { get => _moveSpeed; }
    public float PatrolRange { get => _patrolRange; }
    public float ChaseRange { get => _chaseRange; }
    public float AttackRange { get => _attackRange; }
    public float ChargeRange { get => _chargeRange; }
    public bool UsePatrol { get => _usePatrol; }
    public int Gold { get => _gold; }
    public int Exp { get => _exp; }
    public bool IsDead { get => _isDead; }


    public void EnableStats()
    {
        if (_monster.Brain.StatData == null)
        {
            //Debug.LogError("[MonsterStats] MonsterStatEntry를 불러오지 못했습니다.");
            enabled = false;
            return;
        }

        InitializeRuntimeStats();
    }

    private void InitializeRuntimeStats()
    {
        float Coeff = GameManager.StageManager.DangerIndexManager.GetDangerIndex();
        _maxHP = Mathf.RoundToInt(_monster.Brain.StatData.maxHP * Coeff);
        _damage = Mathf.RoundToInt(_monster.Brain.StatData.damage * Coeff);

        if (_monster.AttackBox != null)
        {
            _monster.AttackBox.SetDamage(_damage);
        }
        _gold = Mathf.RoundToInt(_monster.Brain.StatData.goldReward * Coeff);
        _exp = Mathf.RoundToInt(_monster.Brain.StatData.expReward * Coeff);
        _moveSpeed = 1f + _monster.Brain.StatData.moveSpeed * 0.1f;
        _moveSpeed = 1f + _monster.Brain.StatData.moveSpeed * 0.1f;
        _patrolRange = _monster.Brain.StatData.patrolRange * 0.25f;
        _chaseRange = _monster.Brain.StatData.chaseRange * 0.25f;
        _attackRange = _monster.Brain.StatData.attackRange * 0.25f;
        _chargeRange = _monster.Brain.StatData.chargeRange * 0.25f;
        _usePatrol = _monster.Brain.StatData.usePatrol;
        _currentHP = _maxHP;
        _isDead = false;
    }

    public void SetHP(int newHP)
    {
        _currentHP = Mathf.Clamp(newHP, 0, _maxHP);
    }

    public void KillIfDead()
    {
        if (_currentHP <= 0 && _monster.Brain != null && !_isDead)
        {
            _isDead = true;
            _monster.Brain.ChangeState(StateID.Dead);
        }

    }

}
