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


    public void EnableStats()
    {
        if (_monster.MonsterBrain.StatData == null)
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
        _maxHP = Mathf.RoundToInt(_monster.MonsterBrain.StatData.maxHP * Coeff);
        _damage = Mathf.RoundToInt(_monster.MonsterBrain.StatData.damage * Coeff);
        _gold = Mathf.RoundToInt(_monster.MonsterBrain.StatData.goldReward * Coeff);
        _exp = Mathf.RoundToInt(_monster.MonsterBrain.StatData.expReward * Coeff);
        _moveSpeed = 1f + _monster.MonsterBrain.StatData.moveSpeed * 0.1f;
        _moveSpeed = 1f + _monster.MonsterBrain.StatData.moveSpeed * 0.1f;
        _patrolRange = _monster.MonsterBrain.StatData.patrolRange * 0.25f;
        _chaseRange = _monster.MonsterBrain.StatData.chaseRange * 0.25f;
        _attackRange = _monster.MonsterBrain.StatData.attackRange * 0.25f;
        _chargeRange = _monster.MonsterBrain.StatData.chargeRange * 0.25f;
        _usePatrol = _monster.MonsterBrain.StatData.usePatrol;
        _currentHP = _maxHP;
    }

    public void SetHP(int newHP)
    {
        _currentHP = Mathf.Clamp(newHP, 0, _maxHP);
    }

    public void KillIfDead()
    {
        if (_currentHP <= 0 && _monster.MonsterBrain != null)
            _monster.MonsterBrain.ChangeState(StateID.Dead);
    }

}
