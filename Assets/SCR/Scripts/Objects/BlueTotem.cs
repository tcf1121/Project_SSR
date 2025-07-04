using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Utill;

namespace SCR
{
    public class BlueTotem : InteractionObject
    {
        protected List<AttackItem> _attackItems = new();
        protected List<StatItem> _statItems = new();
        public override void Interaction()
        {
            if (!_isOpen && GameManager.Player.UseMoney(30))
            {
                _isOpen = true;
                Use();
            }
            if (_isOpen)
            {
                GameManager.Player.ConditionalUI.SelectUI.SetItem(_attackItems, _statItems);
                GameManager.Player.ConditionalUI.SelectUI.SetTotem(this.gameObject);
                GameManager.Player.ConditionalUI.SelectUI.OnOffUI(true);
            }
        }

        public override void Use()
        {
            _attackItems.Clear();
            _statItems.Clear();
            List<GameObject> itemList = GameManager.ItemManager.GetItemList(3);
            foreach (GameObject item in itemList)
            {
                if (item.TryGetComponent<AttackItem>(out AttackItem aitem))
                {
                    _attackItems.Add(aitem);
                    _statItems.Add(null);
                }
                else
                {
                    _attackItems.Add(null);
                    _statItems.Add(item.GetComponent<StatItem>());
                }

            }
            GameManager.Player.ConditionalUI.SelectUI.SetItem(_attackItems, _statItems);
            GameManager.Player.ConditionalUI.SelectUI.SetTotem(this.gameObject);
            GameManager.Player.ConditionalUI.SelectUI.OnOffUI(true);
            // 3개의 랜덤 아이템 중 선택할 수 있게하기

        }

        public void Disappear()
        {
            _animator.SetTrigger("Open");
            ObjectPool.ReturnPool(this.gameObject, EPoolObjectType.Object);
        }
    }
}

