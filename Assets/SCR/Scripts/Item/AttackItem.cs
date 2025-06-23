using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCR
{
    public class AttackItem : Item
    {
        // 기본 데미지
        [SerializeField] private float _basicDamage;

        // 현재 데미지
        public float CurrentDamage { get { return _currentDamage; } }
        private float _currentDamage;

        // 강화 비율
        [SerializeField] private float _strengthening;

        // 쿨타임
        public float CoolTime { get { return _currentDamage; } }
        [SerializeField] private float _coolTime;

        // 공격 범위
        [SerializeField] GameObject _attackrange;

        override protected void Init()
        {
            base.Init();
            if (_itemPart == ItemPart.Head) _coolTime = 30f;
            else if (_itemPart == ItemPart.Body) _coolTime = 5f;
            _currentDamage = _basicDamage;
        }

        override protected void ItemEnhancement()
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
    }
}

