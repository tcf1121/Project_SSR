using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utill;


namespace SCR
{
    public class Player : MonoBehaviour
    {
        // ===== Link =====
        private Equipped _equipped;
        public Equipped Equipped { get => _equipped; }
        private PlayerController _playerController;
        public PlayerController PlayerController { get => _playerController; }
        private PlayerPhysical _playerPhysical;
        public PlayerPhysical PlayerPhysical { get => _playerPhysical; }
        private PlayerStats _playerStats;
        public PlayerStats PlayerStats { get => _playerStats; }
        private PlayerWeapon _playerWeapon;
        public PlayerWeapon PlayerWeapon { get => _playerWeapon; }

        // ===== UI =====
        [Header("UI")]
        [SerializeField] private ConditionalUI _conditionalUI;
        [SerializeField] private AlwaysOnUI _alwaysOnUI;
        public ConditionalUI ConditionalUI { get { return _conditionalUI; } }
        public AlwaysOnUI AlwaysOnUI { get { return _alwaysOnUI; } }


        public GameObject WaitItem { get { return _waitItem; } }
        private GameObject _waitItem;

        // ===== 컴포넌트 =====
        public Rigidbody2D Rigid { get { return _rigid; } }
        private Rigidbody2D _rigid;
        public Collider2D Collider { get { return _collider; } }
        private Collider2D _collider;
        public Animator Animator { get { return _animator; } }
        private Animator _animator;




        // ===== 기타 상태 변수 =====
        public float OriginalGravityScale { get { return _originalGravityScale; } }
        private float _originalGravityScale;
        public float OriginalPlayerHeight { get { return _originalPlayerHeight; } }
        private float _originalPlayerHeight;

        private void Awake() => Init();

        private void Init()
        {
            _animator = GetComponent<Animator>();
            _rigid = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _equipped = GetComponent<Equipped>();
            _playerController = GetComponent<PlayerController>();
            _playerPhysical = GetComponent<PlayerPhysical>();
            _playerStats = GetComponent<PlayerStats>();
            _playerWeapon = GetComponent<PlayerWeapon>();
            _alwaysOnUI.LinkedPlayer(this);
            _conditionalUI.LinkedPlayer(this);
            _originalGravityScale = _rigid.gravityScale;
            _originalPlayerHeight = _collider.bounds.size.y;
        }

        private void Start()
        {
            // 발밑에 바닥 체크 자동으로 생성
            if (_playerPhysical.GroundCheck == null)
            {
                GameObject child = new GameObject("GroundCheckPos");
                child.transform.parent = this.transform;

                Collider2D col = GetComponent<Collider2D>();
                if (col != null)
                {
                    float bottomY = col.bounds.min.y;
                    Vector3 localBottom = transform.InverseTransformPoint(new Vector3(transform.position.x, bottomY, transform.position.z));
                    child.transform.localPosition = new Vector3(0, localBottom.y, 0);
                }
                else
                {
                    child.transform.localPosition = Vector3.zero;
                }
            }
            _playerPhysical.SetGroundCheck(transform.Find("GroundCheckPos"));
        }

        public void Equip(GameObject item)
        {
            bool IsAttackItem = item.GetComponent<AttackItem>() != null ? true : false;
            if (_equipped.CheckItem(item))
            {

                ObjectPool.ReturnPool(item, IsAttackItem ? EPoolObjectType.AttackItem : EPoolObjectType.StatItem);
                _conditionalUI.EnhancementInfo.SetItem(item);
                _conditionalUI.EnhancementInfo.GetItem();
            }
            else
            {
                if (_equipped.CheckFull(item))
                {
                    _equipped.EquipItem(item);


                    ObjectPool.ReturnPool(item, IsAttackItem ? EPoolObjectType.AttackItem : EPoolObjectType.StatItem);
                    _conditionalUI.ItemInfoUI.SetItem(item);
                    _conditionalUI.ItemInfoUI.GetItem();
                    if (item.GetComponent<AttackItem>())
                        _playerWeapon.AddWeapon(item.GetComponent<AttackItem>());
                    else
                        _playerStats.BonusStats.AddStats(item.GetComponent<StatItem>().CurrentStat);
                }
                else
                {
                    //교체하기
                    ObjectPool.ReturnPool(item, IsAttackItem ? EPoolObjectType.AttackItem : EPoolObjectType.StatItem);
                    _waitItem = item;
                    _conditionalUI.EquipUI.gameObject.SetActive(true);
                    _conditionalUI.EquipUI.IsChangeEquip(true);
                    _conditionalUI.EquipUI.OpenEquip((int)item.GetComponent<Item>().ItemPart);
                    Time.timeScale = 0f;

                }
            }
        }
    }
}

