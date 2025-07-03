using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCR
{
    public class AttackItem : Item
    {
        // 기본 데미지
        public float BasicDamage { get => _basicDamage; }
        [SerializeField] private float _basicDamage;

        // 현재 데미지
        public float CurrentDamage { get { return _currentDamage; } }
        private float _currentDamage;

        // 강화 비율
        public float Strengthening { get => _strengthening; }
        [SerializeField] private float _strengthening;

        // 쿨타임
        public float CoolTime { get { return _coolTime; } }
        [SerializeField] private float _coolTime;

        // 공격 범위
        public GameObject Attackrange { get => _attackrange; }
        [SerializeField] private GameObject _attackrange;

        override protected void Init()
        {
            base.Init();
            if (_itemPart == ItemPart.Head) _coolTime = 10f;
            else if (_itemPart == ItemPart.Body) _coolTime = 5f;
            _currentDamage = _basicDamage;
        }

        override public void ItemEnhancement()
        {
            base.ItemEnhancement();
            _currentDamage += _strengthening;
        }

        public void Attack()
        {
            _attackrange.SetActive(true);
        }

        public void EndAttack()
        {
            _attackrange.SetActive(false);
        }

        public void Clone(AttackItem attackItem, bool isGameobject = true)
        {
            _image = attackItem.Image;
            _itemPart = attackItem.ItemPart;
            _code = attackItem.Code;
            _name = attackItem.Name;
            _description = attackItem.Description;
            _unlockDescription = attackItem.Description;
            _isUnlock = attackItem.IsUnlock;
            itemPrefab = attackItem.itemPrefab;
            _basicDamage = attackItem.BasicDamage;
            _strengthening = attackItem.Strengthening;
            _coolTime = attackItem.CoolTime;
            _attackrange = attackItem.Attackrange;
            if (isGameobject)
            {
                gameObject.transform.localScale = attackItem.gameObject.transform.localScale;
                gameObject.GetComponent<BoxCollider2D>().offset = attackItem.gameObject.GetComponent<BoxCollider2D>().offset;
                gameObject.GetComponent<BoxCollider2D>().size = attackItem.gameObject.GetComponent<BoxCollider2D>().size;
                gameObject.GetComponent<SpriteRenderer>().sprite = _image;
            }
        }
    }
}

