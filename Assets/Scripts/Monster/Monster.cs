using UnityEngine;
using UnityEngine.UI;

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
    public MonsterBrain MonsterBrain { get => _monsterBrain; }
    public MonsterStats MonsterStats { get => _monsterStats; }
    public SpriteRenderer Sprite { get => _sprite; }
    public BoxCollider2D HitBox { get => _hitBox; }
    public BoxCollider2D AttackBox { get => _attackBox; }
    public Animator Animator { get => _animator; }
    public Transform MuzzlePoint { get => _muzzlePoint; }
    public Transform GroundSensor { get => _groundSensor; }
    public Transform WallSensor { get => _wallSensor; }
    public Rigidbody2D Rigid { get => _rigid; }
    public Transform Transfrom { get => _transfrom; }
    public RectTransform HpBar { get => _hpBar; }
    public Image HpBarFill { get => _hpBarFill; }


    [SerializeField] private MonsterType _monsterType;
    [SerializeField] private MonsterSpecies _monsterSpecies;
    [SerializeField] private AllMonsterStatData _allMonsterStatData;
    [SerializeField] private int _creadit;
    [SerializeField] private MonsterBrain _monsterBrain;
    [SerializeField] private MonsterStats _monsterStats;
    [SerializeField] private SpriteRenderer _sprite;
    [SerializeField] private BoxCollider2D _hitBox;
    [SerializeField] private BoxCollider2D _attackBox;
    [SerializeField] private Animator _animator;
    [SerializeField] private Transform _muzzlePoint;
    [SerializeField] private Transform _groundSensor;
    [SerializeField] private Transform _wallSensor;
    [SerializeField] private Rigidbody2D _rigid;
    [SerializeField] private Transform _transfrom;
    [SerializeField] private RectTransform _hpBar;
    [SerializeField] private Image _hpBarFill;









    public void Clone(Monster monster)
    {
        _hitBox.offset = monster.HitBox.offset;
        _hitBox.isTrigger = monster.HitBox.isTrigger;
        _hitBox.size = monster.HitBox.size;
        _sprite.sprite = monster.Sprite.sprite;
        _sprite.flipX = monster.Sprite.flipX;

        _animator.runtimeAnimatorController = monster.Animator.runtimeAnimatorController;
        _monsterBrain.Clone(monster.MonsterBrain);
        _monsterBrain.enabled = true;
        _monsterStats.enabled = true;
        _monsterType = monster.MonsterType;
        _allMonsterStatData = monster.AllMonsterStatData;
        _monsterSpecies = monster.MonsterSpecies;
        _creadit = monster.Credit;
        _groundSensor.position = monster.GroundSensor.position;
        _wallSensor.position = monster.WallSensor.position;
        if (MonsterType != MonsterType.CDMonster)
        {
            if (monster.MuzzlePoint != null)
                _muzzlePoint.position = monster.MuzzlePoint.position;
        }
        _hpBar.sizeDelta = monster.HpBar.sizeDelta;
        _hpBar.position = monster.HpBar.position;
        gameObject.transform.position = monster.gameObject.transform.position;
        gameObject.transform.localScale = monster.gameObject.transform.localScale;

        _attackBox.offset = monster.AttackBox.offset;
        _attackBox.isTrigger = monster.AttackBox.isTrigger;
        _attackBox.size = monster.AttackBox.size;
        _attackBox.gameObject.transform.position = monster.AttackBox.gameObject.transform.position;
    }
}

