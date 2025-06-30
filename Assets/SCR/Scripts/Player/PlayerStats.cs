using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace SCR
{
    [System.Serializable]
    public struct Stats
    {
        public float MaxHp;
        public float Atk;
        public float HpRegen;
        public float Speed;
        public float Jump;

        public void BaseStats()
        {
            MaxHp = 100f;
            Atk = 10f;
            HpRegen = 0.2f;
            Speed = 7f;
            Jump = 1f;
        }

        public void ResetStats()
        {
            MaxHp = 0;
            Atk = 0;
            HpRegen = 0;
            Speed = 0;
            Jump = 0;
        }

        public void AddStats(Stats stats)
        {
            MaxHp += stats.MaxHp;
            Atk += stats.Atk;
            HpRegen += stats.HpRegen;
            Speed += stats.Speed;
            Jump += stats.Jump;
        }

        public void SubStats(Stats stats)
        {
            MaxHp -= stats.MaxHp;
            Atk -= stats.Atk;
            HpRegen -= stats.HpRegen;
            Speed -= stats.Speed;
            Jump -= stats.Jump;
        }

        public void LevelUp()
        {
            MaxHp += 33f;
            Atk += 2f;
            HpRegen += 0.2f;
        }

        public void FinalStats(Stats baseStats, Stats bonusStats)
        {
            MaxHp = baseStats.MaxHp + bonusStats.MaxHp;
            Atk = baseStats.Atk + bonusStats.Atk;
            HpRegen = baseStats.HpRegen + bonusStats.HpRegen;
            Speed = baseStats.Speed + bonusStats.Speed;
            Jump = baseStats.Jump + bonusStats.Jump;
        }

        public void Enhancement(Stats baseStats, float Strengthening)
        {
            MaxHp = baseStats.MaxHp + (int)(baseStats.MaxHp * Strengthening);
            Atk = baseStats.Atk + (int)(baseStats.Atk * Strengthening);
            HpRegen = baseStats.HpRegen + (int)(baseStats.HpRegen * Strengthening);
            Speed = baseStats.Speed + (int)(baseStats.Speed * Strengthening);
            Jump = baseStats.Jump + (int)(baseStats.Jump * Strengthening);
        }
    }

    public class PlayerStats : MonoBehaviour
    {
        private Player player;

        [Header("�ɸ��� �⺻ ����")] // �Ŀ� �����̺����� ����
        [SerializeField] private int _level = 1;
        [SerializeField] private float _currentHp;
        [SerializeField] private int _currentExp;
        [SerializeField] private int _reqExp;
        [SerializeField] private int _money = 0;

        [Header("�⺻ ����")]
        [SerializeField] private Stats _baseStats;

        [Header("���ʽ� ���� (���/����)")]
        [SerializeField] private Stats _bonusStats;
        public Stats BonusStats { get => _bonusStats; }

        [Header("���� ����")]
        [SerializeField] private Stats _finalStats;

        // ü�� ��� Ÿ�̸�
        private float regenTimer = 0f;

        // ���� �б� ���� ����
        public int Level { get { return _level; } }
        public float CurrentHp { get { return _currentHp; } set { _currentHp = value; _changeHp?.Invoke(); } }
        public int CurrentExp { get => _currentExp; set { _currentExp = value; _changeExp?.Invoke(); } }
        public int Money { get => _money; set { _money = value; _changeMoney?.Invoke(); } }
        public Stats FinalStats { get => _finalStats; }
        public float ReqExp { get { return _reqExp; } }
        private UnityAction _changeHp;
        private UnityAction _changeExp;
        private UnityAction _changeMoney;
        private UnityAction _changeStats;
        private UnityAction _changeLevel;
        private UnityAction _isDead;

        private Coroutine _hpRegenCor;

        void Awake()
        {
            player = GetComponent<Player>();
            _baseStats.BaseStats();
            _bonusStats.ResetStats();
            _changeStats += SetFinalStats;
            _changeStats?.Invoke();
            _changeHp += SetHp;
            _changeExp += LevelUpCheck;
            _changeExp += SetExp;
            _changeMoney += SetMoney;
            _changeLevel += SetLevel;
            _isDead += Die;
            Money = 0;
        }

        void Start()
        {
            FullHPRecovery();
            RequiredExp();
            CurrentExp = 0;
            _hpRegenCor = StartCoroutine(HpRegen());
        }

        private void Update()
        {

        }

        #region ����ġ �� ���� ����
        public void LevelUpCheck()
        {
            while (CurrentExp >= _reqExp)
            {
                CurrentExp -= _reqExp;
                LevelUp();
            }
        }

        public void LevelUp()
        {
            _level++;
            RequiredExp(); // �ʿ����ġ ����
            _baseStats.LevelUp();
            CurrentHp = _finalStats.MaxHp;
            _changeLevel?.Invoke();
            _changeStats?.Invoke();
            // ���� ���� �˸� OnLevelUp?.Invoke(level);
        }

        /// <summary>
        /// �ʿ� ����ġ ��� (�Ҽ� ù�ڸ����� �ݿø�)
        /// </summary>
        public void RequiredExp()
        {
            float requiredExp = 30f; // 1���� �⺻ ����ġ

            for (int i = 2; i <= _level; i++)
            {
                requiredExp *= 1.6f;
            }

            _reqExp = (int)Mathf.Round(requiredExp);
        }
        #endregion

        #region ���� ����
        public void Die()
        {
            StopCoroutine(_hpRegenCor);
            Time.timeScale = 0f;
        }


        // ����
        #endregion

        #region HP ����
        /// <summary>
        /// ü�� ȸ��
        /// </summary>
        public void Heal(float healAmount)
        {
            float oldHp = CurrentHp;
            CurrentHp = Mathf.Min(CurrentHp + healAmount, _finalStats.MaxHp);

            if (CurrentHp != oldHp)
            {
                // OnHpChanged?.Invoke(currentHp); ���� ü�� ���� �˸�
            }
        }

        /// <summary>
        /// ������ �ޱ�
        /// </summary>
        public void TakeDamage(float damage)
        {
            float oldHp = CurrentHp;
            CurrentHp = Mathf.Max(CurrentHp - damage, 0f);

            // ���� �ð� ���� ���� (��ȹ ������)
            // ���� (��ȹ ������)
            if (CurrentHp != oldHp)
            {
                // OnHpChanged?.Invoke(currentHp); ���� ü�� ���� �˸�
            }

            if (CurrentHp <= 0)
            {
                _isDead.Invoke();
                // ��� ó�� ����
            }
        }

        /// <summary>
        /// ü�� ���
        /// </summary>
        private IEnumerator HpRegen()
        {
            while (true)
            {
                float RegenTime = 5f;
                Heal(_finalStats.HpRegen);
                while (RegenTime > 0.0f)
                {
                    RegenTime -= Time.deltaTime;
                    yield return new WaitForFixedUpdate();
                }
            }
        }

        /// <summary>
        /// ü�� �ִ����� ȸ��
        /// </summary>
        public void FullHPRecovery()
        {
            CurrentHp = _finalStats.MaxHp;
        }

        // ���°� ������ ����
        /// <summary>
        /// HP ���� ��ȯ (0~1) _��� �����ۿ� ���� ü���� �����ϸ� ������ �����ϸ� �����ϵ��� �� _�ݴ뵵 ��������
        /// </summary>
        public float GetHpRatio()
        {
            if (_finalStats.MaxHp <= 0)
                return 0f;
            return _currentHp / _finalStats.MaxHp;
        }
        #endregion

        #region ��� �Ǵ� ���� �ܺ� ��ҿ� ���� ���� ����
        // ���ʽ� ���Ȱ������� ���� (�������ΰ� �ƴ϶�� ������� �������)
        /// <summary>
        /// ��� ����/����, ���� �� �ܺ� ��ҿ� ���� ���� ����
        /// </summary>
        /// <param name="addstats">������ ���� ����</param>
        /// <param name="equip">���� = true, ���� = false</param>
        public void EquipItem(Stats addstats, bool equip = true)
        {
            if (equip) _bonusStats.AddStats(addstats);
            else _bonusStats.SubStats(addstats);
            _changeStats?.Invoke();
        }

        private void SetFinalStats()
        {
            _finalStats.FinalStats(_baseStats, _bonusStats);
        }

        #endregion

        #region ����ġ ���
        /// <summary>
        /// ����ġ ���
        /// </summary>
        public void GetExp(int exp)
        {
            CurrentExp += exp;
            LevelUpCheck();
        }

        #endregion



        #region �� ����
        /// <summary>
        /// �� �Ҹ� �� ���
        /// </summary>
        /// <param name="amount">���</param>
        /// <returns>���� ���� ����</returns>
        public bool SpendMoney(int amount)
        {
            if (Money >= amount)
            {
                Money -= amount;
                return true;
            }
            return false;
        }
        #endregion

        #region UI����
        private void SetLevel()
        {
            player.AlwaysOnUI.SetLevel(_level);
        }
        private void SetHp()
        {
            player.AlwaysOnUI.SetHp((int)_currentHp, (int)FinalStats.MaxHp);
        }

        private void SetExp()
        {
            player.AlwaysOnUI.SetExp(_currentExp, _reqExp);
        }

        private void SetMoney()
        {
            player.AlwaysOnUI.SetCoin(_money);
        }
        #endregion
    }
}