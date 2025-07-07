using System.Collections;
using UnityEngine;
using UnityEngine.Events;

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

    public void Enhancement(Stats Strengthening)
    {
        MaxHp += Strengthening.MaxHp;
        Atk += Strengthening.Atk;
        HpRegen += Strengthening.HpRegen;
        Speed += Strengthening.Speed;
        Jump += Strengthening.Jump;
    }
}

public class PlayerStats : MonoBehaviour
{
    private Player player;

    [Header("케릭터 기본 정보")] // 후에 프라이빗으로 변경
    [SerializeField] private int _level = 1;
    [SerializeField] private float _currentHp;
    [SerializeField] private int _currentExp;
    [SerializeField] private int _reqExp;
    [SerializeField] private int _money = 0;

    [Header("기본 스탯")]
    [SerializeField] private Stats _baseStats;

    [Header("보너스 스탯 (장비/버프)")]
    [SerializeField] private Stats _bonusStats;
    public Stats BonusStats { get => _bonusStats; }

    [Header("현재 상태")]
    [SerializeField] private Stats _finalStats;

    public AudioClip DieClip;

    // 체력 재생 타이머
    private float regenTimer = 0f;

    // 변수 읽기 쓰기 관리
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
    private Coroutine _damageCor;

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
        _damageCor = null;
    }

    void Start()
    {
        FullHPRecovery();
        RequiredExp();
        CurrentExp = 0;
        _hpRegenCor = StartCoroutine(HpRegen());
        player.Animator.SetBool("IsAlive", true);
    }

    private void Update()
    {

    }

    #region 경험치 및 레벨 관리
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
        RequiredExp(); // 필요경험치 재계산
        _baseStats.LevelUp();
        CurrentHp = _finalStats.MaxHp;
        _changeLevel?.Invoke();
        _changeStats?.Invoke();
        // 레벨 변경 알림 OnLevelUp?.Invoke(level);
    }

    /// <summary>
    /// 필요 경험치 계산 (소수 첫자리까지 반올림)
    /// </summary>
    public void RequiredExp()
    {
        float requiredExp = 30f; // 1레벨 기본 경험치

        for (int i = 2; i <= _level; i++)
        {
            requiredExp *= 1.6f;
        }

        _reqExp = (int)Mathf.Round(requiredExp);
    }
    #endregion

    #region 생명 관리
    public void Die()
    {
        player.AudioSource.PlayOneShot(DieClip);
        player.Animator.SetBool("IsAlive", false);
        StopCoroutine(_hpRegenCor);
        Time.timeScale = 0f;
    }


    // 리셋
    #endregion

    #region HP 관리
    /// <summary>
    /// 체력 회복
    /// </summary>
    public void Heal(float healAmount)
    {
        float oldHp = CurrentHp;
        CurrentHp = Mathf.Min(CurrentHp + healAmount, _finalStats.MaxHp);

        if (CurrentHp != oldHp)
        {
            // OnHpChanged?.Invoke(currentHp); 현재 체력 변경 알림
        }
    }

    /// <summary>
    /// 데미지 받기
    /// </summary>
    public void TakeDamage(float damage)
    {
        // 일정 시간 무적 구현
        if (_damageCor == null)
        {
            _damageCor = StartCoroutine(DamageCoroutine(damage));
        }
        // 경직 (기획 논의중)
    }

    public IEnumerator DamageCoroutine(float damage)
    {
        float currentTime = 0.5f;
        float oldHp = CurrentHp;
        CurrentHp = Mathf.Max(CurrentHp - damage, 0f);
        if (CurrentHp != oldHp)
        {
            // OnHpChanged?.Invoke(currentHp); 현재 체력 변경 알림
            player.Animator.SetTrigger("Hit");
        }

        if (CurrentHp <= 0)
        {
            _isDead.Invoke();
            // 사망 처리 로직
        }
        while (currentTime > 0.0f)
        {
            currentTime -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        StopCoroutine(_damageCor);
        _damageCor = null;
    }

    public void UseHp(int hp)
    {
        CurrentHp -= hp;
    }

    /// <summary>
    /// 체력 재생
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
    /// 체력 최대으로 회복
    /// </summary>
    public void FullHPRecovery()
    {
        CurrentHp = _finalStats.MaxHp;
    }

    // 없는게 맞을거 같음
    /// <summary>
    /// HP 비율 반환 (0~1) _장비 아이템에 의해 체력이 증가하면 비율을 유지하며 증가하도록 함 _반대도 마찬가지
    /// </summary>
    public float GetHpRatio()
    {
        if (_finalStats.MaxHp <= 0)
            return 0f;
        return _currentHp / _finalStats.MaxHp;
    }

    public int GetCurrentHpRatio(float Ratio)
    {
        return (int)(_currentHp * Ratio);
    }
    #endregion

    #region 장비 또는 버프 외부 요소에 의한 스탯 증감
    // 보너스 스탯가져가서 수정 (영구적인게 아니라면 원래대로 해줘야함)
    /// <summary>
    /// 장비 장착/해제, 버프 등 외부 요소에 의한 스탯 증감
    /// </summary>
    /// <param name="addstats">전달할 스탯 정보</param>
    /// <param name="equip">장비 장착 = true, 버프는 =false</param>
    public void EquipItem(Stats addstats, bool equip = true, float bufftime = 0f)
    {
        if (equip)
        {
            _bonusStats = addstats;
            _changeStats?.Invoke();
        }
        else
        {
            StartCoroutine(Buff(addstats, bufftime));
        }

    }

    private IEnumerator Buff(Stats buffStats, float bufftime)
    {
        _bonusStats.AddStats(buffStats);
        _changeStats?.Invoke();
        float currentTime = bufftime;
        while (currentTime > 0.0f)
        {
            currentTime -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        _bonusStats.SubStats(buffStats);
        _changeStats?.Invoke();
    }

    private void SetFinalStats()
    {
        _finalStats.FinalStats(_baseStats, _bonusStats);
        player.PlayerPhysical.SetSpeed();
        player.PlayerPhysical.SetJump();
    }

    #endregion

    #region 경험치 얻기
    /// <summary>
    /// 경험치 얻기
    /// </summary>
    public void GetExp(int exp)
    {
        CurrentExp += exp;
        LevelUpCheck();
    }


    #endregion



    #region 돈 관리
    public void GetMoney(int gold)
    {
        Money += gold;
    }

    /// <summary>
    /// 돈 소모 및 사용
    /// </summary>
    /// <param name="amount">비용</param>
    /// <returns>결제 성공 여부</returns>
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

    #region UI연동
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