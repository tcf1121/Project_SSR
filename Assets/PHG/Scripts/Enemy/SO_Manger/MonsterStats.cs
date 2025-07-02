using UnityEngine;

namespace PHG
{
    public class MonsterStats : MonoBehaviour
    {

        [SerializeField] private MonsterBrain brain;

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

        public int MaxHP { get => _maxHP; }
        public int CurrentHP { get => _currentHP; }
        public int Damage { get => _damage; }
        public float MoveSpeed { get => _moveSpeed; }
        public float PatrolRange { get => _patrolRange; }
        public float ChaseRange { get => _chaseRange; }
        public float AttackRange { get => _attackRange; }
        public float ChargeRange { get => _chargeRange; }
        public bool UsePatrol { get => _usePatrol; }

        private void Awake()
        {
            brain = GetComponent<MonsterBrain>();
        }

        public void EnableStats()
        {
            if (brain.StatData == null)
            {
                Debug.LogError("[MonsterStats] MonsterStatEntry를 불러오지 못했습니다.");
                enabled = false;
                return;
            }

            Debug.Log(brain.StatData.monsterType);
            InitializeRuntimeStats();
        }

        private void InitializeRuntimeStats()
        {
            _maxHP = Mathf.RoundToInt(brain.StatData.maxHP * brain.Coeff);
            Debug.Log(_maxHP);
            _damage = Mathf.RoundToInt(brain.StatData.damage * brain.Coeff);
            Debug.Log(_damage);
            _moveSpeed = brain.StatData.moveSpeed;
            Debug.Log(_moveSpeed);
            _patrolRange = brain.StatData.patrolRange;
            _chaseRange = brain.StatData.chaseRange;
            _attackRange = brain.StatData.attackRange;
            _chargeRange = brain.StatData.chargeRange;
            _usePatrol = brain.StatData.usePatrol;
            _currentHP = _maxHP;
        }

        public void SetHP(int newHP)
        {
            _currentHP = Mathf.Clamp(newHP, 0, _maxHP);
        }

        public void KillIfDead()
        {
            if (_currentHP <= 0 && brain != null)
                brain.ChangeState(StateID.Dead);
        }

    }
}
