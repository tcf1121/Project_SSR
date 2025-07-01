using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCR
{
    [System.Serializable]
    public struct ItemPartProbability
    {
        public int Head;
        public int Body;
        public int Arm;
        public int Leg;
        public int[] Sum;

        public void SetSum()
        {
            Sum = new int[4];
            Sum[0] = Head;
            Sum[1] = Head + Body;
            Sum[2] = Head + Body + Arm;
            Sum[3] = Head + Body + Arm + Leg;
        }
    }
    public class ItemManager : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _itemPrefab;
        private List<AttackItem> _headItem;
        private List<AttackItem> _bodyItem;
        private List<AttackItem> _armItem;
        private List<StatItem> _legItem;
        [SerializeField] ItemPartProbability _probability;

        public AttackItem AttackItem { get => _attackItem; }
        private AttackItem _attackItem;
        public StatItem StatItem { get => _statItem; }
        private StatItem _statItem;

        void Awake() => Init();

        private void Init()
        {
            ClassificationItem();
        }

        /// <summary>
        /// 프리팹에 들어있는 아이템들을 부위별로 나눈다.
        /// </summary>
        private void ClassificationItem()
        {
            _headItem = new();
            _bodyItem = new();
            _armItem = new();
            _legItem = new();
            foreach (GameObject item in _itemPrefab)
            {
                if (item.GetComponent<AttackItem>() != null)
                {
                    AttackItem attackItem = item.GetComponent<AttackItem>();
                    switch (attackItem.ItemPart)
                    {
                        case ItemPart.Head:
                            _headItem.Add(attackItem);
                            break;
                        case ItemPart.Body:
                            _bodyItem.Add(attackItem);
                            break;
                        case ItemPart.Arm:
                            _armItem.Add(attackItem);
                            break;
                    }
                }
                else
                {
                    _legItem.Add(item.GetComponent<StatItem>());
                }
            }
        }

        public bool IsAttackItem()
        {
            return _attackItem != null ? true : false;
        }

        public void PickItem()
        {
            SetItem(PickItemPart());
        }

        private ItemPart PickItemPart()
        {
            ItemPart part;
            _probability.SetSum();
            int randomNum = Random.Range(0, _probability.Sum[3]);
            if (randomNum < _probability.Sum[0])
                part = ItemPart.Head;
            else if (randomNum < _probability.Sum[1])
                part = ItemPart.Body;
            else if (randomNum < _probability.Sum[2])
                part = ItemPart.Arm;
            else
                part = ItemPart.Leg;

            return part;
        }

        private void SetItem(ItemPart itemPart)
        {
            _attackItem = null;
            _statItem = null;
            switch (itemPart)
            {
                case ItemPart.Head:
                    _attackItem = _headItem[Random.Range(0, _headItem.Count)];
                    break;
                case ItemPart.Body:
                    _attackItem = _bodyItem[Random.Range(0, _bodyItem.Count)];
                    break;
                case ItemPart.Arm:
                    _attackItem = _armItem[Random.Range(0, _armItem.Count)];
                    break;
                case ItemPart.Leg:
                    _statItem = _legItem[Random.Range(0, _legItem.Count)];
                    break;
            }
        }
    }

}
