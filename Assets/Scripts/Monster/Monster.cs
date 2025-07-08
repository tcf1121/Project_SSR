using System.Collections;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UI;
using Utill;

public enum MonsterType
{
    CDMonster,
    LDMonster,
    FlyMonster
}
public class Monster : MonoBehaviour
{
    public MonsterType MonsterType { get => _monsterType; }
    public MonsterSpecies MonsterSpecies { get => _monsterSpecies; }
    public AllMonsterStatData AllMonsterStatData { get => _allMonsterStatData; }
    public int Credit { get => _creadit; }
    public MonsterBrain Brain { get => _monsterBrain; }
    public MonsterStats MonsterStats { get => _monsterStats; }
    public SpriteRenderer Sprite { get => _sprite; }
    public BoxCollider2D HitBox { get => _hitBox; }
    public BoxCollider2D AttackBoxCol { get => _attackBoxCol; }
    public AtttackBox AttackBox { get => _attackBox; }
    public Animator Animator { get => _animator; }
    public Transform MuzzlePoint { get => _muzzlePoint; }
    public Transform GroundSensor { get => _groundSensor; }
    public Transform WallSensor { get => _wallSensor; }
    public Rigidbody2D Rigid { get => _rigid; }
    public Transform Transfrom { get => _transfrom; }
    public RectTransform HpBar { get => _hpBar; }
    public Image HpBarFill { get => _hpBarFill; }
    public Transform Target { get => _target; }

    [SerializeField] private MonsterType _monsterType;
    [SerializeField] private MonsterSpecies _monsterSpecies;
    [SerializeField] private AllMonsterStatData _allMonsterStatData;
    [SerializeField] private int _creadit;
    [SerializeField] private MonsterBrain _monsterBrain;
    [SerializeField] private MonsterStats _monsterStats;
    [SerializeField] private SpriteRenderer _sprite;
    [SerializeField] private BoxCollider2D _hitBox;
    [SerializeField] private BoxCollider2D _attackBoxCol;
    [SerializeField] private AtttackBox _attackBox;
    [SerializeField] private Animator _animator;
    [SerializeField] private Transform _muzzlePoint;
    [SerializeField] private Transform _groundSensor;
    [SerializeField] private Transform _wallSensor;
    [SerializeField] private Rigidbody2D _rigid;
    [SerializeField] private Transform _transfrom;
    [SerializeField] private RectTransform _hpBar;
    [SerializeField] private Image _hpBarFill;
    private Transform _target;


    public void Clone(Monster monster)
    {
        if (GameManager.Player != null)
            _target = GameManager.Player.transform;
        _hitBox.offset = monster.HitBox.offset;
        _hitBox.isTrigger = monster.HitBox.isTrigger;
        _hitBox.size = monster.HitBox.size;
        _sprite.sprite = monster.Sprite.sprite;
        _sprite.flipX = monster.Sprite.flipX;

        _animator.runtimeAnimatorController = monster.Animator.runtimeAnimatorController;
        _monsterType = monster.MonsterType;
        _allMonsterStatData = monster.AllMonsterStatData;
        _monsterSpecies = monster.MonsterSpecies;
        _monsterBrain.SetBrain();
        _monsterStats.enabled = true;
        _creadit = monster.Credit;
        _groundSensor.position = monster.GroundSensor.position;
        _wallSensor.position = monster.WallSensor.position;
        if (monster.MonsterType != MonsterType.CDMonster)
        {
            if (monster.MuzzlePoint != null)
                _muzzlePoint.position = monster.MuzzlePoint.position;
        }
        _hpBar.sizeDelta = monster.HpBar.sizeDelta;
        _hpBar.position = monster.HpBar.position;
        gameObject.transform.position = monster.gameObject.transform.position;
        gameObject.transform.localScale = monster.gameObject.transform.localScale;

        if (monster.MonsterType != MonsterType.LDMonster)
        {
            _attackBoxCol.offset = monster.AttackBoxCol.offset;
            _attackBoxCol.isTrigger = monster.AttackBoxCol.isTrigger;
            _attackBoxCol.size = monster.AttackBoxCol.size;
            _attackBoxCol.gameObject.transform.position = monster.AttackBoxCol.gameObject.transform.position;
            _attackBox = monster.AttackBox;
        }

    }

    public void PlayAnim(string animName)
    {
        Debug.Log($"[FSM] {Brain.name} Current State: {Brain.StateMachine.CurrentStateID}");
        Animator.Play(animName);
    }

    public void Death()
    {
        // 1. 보상 계산
        GameManager.Player.GetReward(MonsterStats.Gold, MonsterStats.Exp);
        // 2. 사망 애니메이션 + 비활성화 처리
        StartCoroutine(DieCoroutine());
    }

    private IEnumerator DieCoroutine()
    {
        if (Brain.StatData.hasIdleAnim)
            PlayAnim(AnimNames.Dead);


        yield return new WaitForSeconds(1.5f); // 사망 연출 대기

        gameObject.SetActive(false); // 오브젝트 풀 반환
        if (_monsterType == MonsterType.FlyMonster) ObjectPool.ReturnPool(gameObject, EPoolObjectType.FlyMonster);
        else if (_monsterType == MonsterType.LDMonster) ObjectPool.ReturnPool(gameObject, EPoolObjectType.LDMonster);
        else ObjectPool.ReturnPool(gameObject, EPoolObjectType.CDMonster);

    }

    public void ChangeState(StateID id)
    {
        Brain.ChangeState(id);
    }

    public float DistanceTarget()
    {
        return Vector2.Distance(_target.transform.position, gameObject.transform.position);
    }

    public bool PlayerInRange(float range)
    {
        if (_target == null) return false;
        return DistanceTarget() <= range ? true : false;
    }

    public int LookAtPlayerDirection()
    {
        Vector2 toPl = _target.position - transform.position;
        return (toPl.x > 0) ? 1 : -1;
    }

    public void FlipMonster()
    {
        int dir = LookAtPlayerDirection();
        _transfrom.localScale = new Vector3(Mathf.Abs(_transfrom.localScale.x) * dir, _transfrom.localScale.y, _transfrom.localScale.z);
    }

    public float LookAtPlayerYPos()
    {
        Vector2 toPl = _target.position - transform.position;
        return toPl.y;
    }

    public void KnockBack()
    {
        Vector2 knockBackDirection = (transform.position - _target.position).normalized;
        _rigid.AddForce(knockBackDirection * 1f, ForceMode2D.Impulse);
        PlayAnim(AnimNames.Stagger);
    }

    public void GetDamage(int damage)
    {
        _monsterBrain.EnterDamageState(damage);
    }
}

