using System;
using System.Collections;
using System.Collections.Generic;
using SCR;
using UnityEngine;

namespace SCR
{
    public class PlayerWeapon : MonoBehaviour
    {
        private Player player;

        [SerializeField] private GameObject _headObj;
        [SerializeField] private GameObject _bodyObj;
        [SerializeField] private GameObject _armObj;
        [SerializeField] private List<Weapon> _headWeapons;
        [SerializeField] private List<Weapon> _bodyWeapons;
        [SerializeField] private List<Weapon> _armWeapons;
        [SerializeField] private List<Coroutine> _armCor;
        private Coroutine _attackCor;

        void Awake() => Init();

        private void Init()
        {
            player = GetComponent<Player>();
            _headWeapons = new();
            _bodyWeapons = new();
            _armWeapons = new();
            _armCor = new();
        }

        public void AddWeapon(String weaponId)
        {
            GameObject weaponObj = Resources.Load(weaponId) as GameObject;
            Weapon weapon = weaponObj.GetComponent<Weapon>();
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
            if (_armWeapons.Count == 0) return;

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
                Debug.Log("공격");
                for (int i = 0; i < _armWeapons.Count; i++)
                    if (_armCor[i] == null)
                        _armCor[i] = StartCoroutine(NormalAttack(i));
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
    }

}
