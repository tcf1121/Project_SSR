using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    private Player player;

    [SerializeField] private GameObject _headObj;
    [SerializeField] private GameObject _bodyObj;
    [SerializeField] private GameObject _armObj;
    [SerializeField] private Weapon _baseWeapon;
    [SerializeField] private List<Weapon> _headWeapons;
    [SerializeField] private List<Weapon> _bodyWeapons;
    [SerializeField] private List<Weapon> _armWeapons;
    public List<Weapon> HeadWeapons { get => _headWeapons; }
    public List<Weapon> BodyWeapons { get => _bodyWeapons; }
    public List<Weapon> ArmWeapons { get => _armWeapons; }
    [SerializeField] private List<Coroutine> _armCor;
    private Coroutine _baseCor;
    private Coroutine _attackCor;

    void Awake() => Init();

    private void Init()
    {
        player = GetComponent<Player>();
        _headWeapons = new();
        _bodyWeapons = new();
        _armWeapons = new();
        _armCor = new();
        _baseCor = null;
    }

    public void AddWeapon(AttackItem item)
    {
        GameObject weaponObj = Instantiate(item.Attackrange);

        Weapon weapon = weaponObj.GetComponent<Weapon>();
        weapon.SetPlayer(player);
        if (weapon.ItemPart == ItemPart.Head)
        {
            weaponObj.transform.parent = _headObj.transform;

            _headWeapons.Add(weapon);
        }
        else if (weapon.ItemPart == ItemPart.Body)
        {
            weaponObj.transform.parent = _bodyObj.transform;
            _bodyWeapons.Add(weapon);
        }
        else if (weapon.ItemPart == ItemPart.Arm)
        {
            weaponObj.transform.parent = _armObj.transform;
            _armWeapons.Add(weapon);
            _armCor.Add(null);
        }
        weaponObj.transform.localPosition = new Vector2(0, 0);
        Vector3 scale = weaponObj.transform.localScale;
        scale.x *= -1f;
        if (!player.PlayerController.FacingRight)
            weaponObj.transform.localScale = scale;
    }

    public void RemoveAtWeapon(ItemPart itemPart, int index)
    {
        if (itemPart == ItemPart.Head)
        {
            Destroy(_headWeapons[index].gameObject);
            _headWeapons.RemoveAt(index);
        }
        else if (itemPart == ItemPart.Body)
        {
            Destroy(_bodyWeapons[index].gameObject);
            _bodyWeapons.RemoveAt(index);
        }
        else if (itemPart == ItemPart.Arm)
        {
            Destroy(_armWeapons[index].gameObject);
            _armWeapons.RemoveAt(index);
        }
    }

    /// <summary>
    /// 기본 공격 실행
    /// </summary>
    public void UseNomalAttack(bool IsAttack)
    {
        if (IsAttack) _attackCor = StartCoroutine(AttackCor());
        else StopCoroutine(_attackCor);
    }

    /// <summary>
    /// 스킬 실행
    /// </summary>
    public void UseSkill()
    {
        if (_bodyWeapons.Count == 0) return;
        foreach (Weapon weapon in _bodyWeapons)
            weapon.Attack();
    }

    /// <summary>
    /// 궁극기 실행
    /// </summary>
    public void UseUltimateSkill()
    {
        if (_headWeapons.Count == 0) return;
        foreach (Weapon weapon in _headWeapons)
            weapon.Attack();
    }

    private IEnumerator AttackCor()
    {
        while (true)
        {
            if (_armWeapons.Count != 0)
                for (int i = 0; i < _armWeapons.Count; i++)
                {
                    if (_armCor[i] == null)
                        _armCor[i] = StartCoroutine(NormalAttack(i));
                }
            if (_baseCor == null)
                _baseCor = StartCoroutine(BaseAttack());
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator NormalAttack(int index)
    {
        float attackCycle = _armWeapons[index].AttackCycle;
        while (attackCycle > 0.0f)
        {
            attackCycle -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        _armWeapons[index].Attack();
        StopCoroutine(_armCor[index]);
        _armCor[index] = null;

    }

    private IEnumerator BaseAttack()
    {
        float attackCycle = _baseWeapon.AttackCycle;
        while (attackCycle > 0.0f)
        {
            attackCycle -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        player.Animator.SetTrigger("Attack");
        _baseWeapon.Attack();
        StopCoroutine(_baseCor);
        _baseCor = null;
    }
}
