using System.Collections.Generic;
using UnityEngine;

namespace SCR
{
    public class Equipped : MonoBehaviour
    {
        private Player player;
        private int _headMaxNum;
        private int _bodyMaxNum;
        private int _armMaxNum;

        public List<GameObject> Head { get { return _head; } }
        [SerializeField] private List<GameObject> _head;

        public List<GameObject> Body { get { return _body; } }
        [SerializeField] private List<GameObject> _body;

        public List<GameObject> Arm { get { return _arm; } }
        [SerializeField] private List<GameObject> _arm;

        public List<GameObject> Leg { get { return _leg; } }
        [SerializeField] private List<GameObject> _leg;

        private void Awake() => Init();

        private void Init()
        {
            player = GetComponent<Player>();
            _head = new();
            _body = new();
            _arm = new();
            _leg = new();

            _headMaxNum = 1;
            _bodyMaxNum = 3;
            _armMaxNum = 5;
        }


        // 아이템 꽉 차 있는지 확인
        public bool CheckItem(GameObject item)
        {
            Item ObjItem = item.GetComponent<Item>();
            switch (ObjItem.ItemPart)
            {
                case ItemPart.Head:
                    if (_head.Count < _headMaxNum)
                        return true;
                    break;
                case ItemPart.Body:
                    if (_body.Count < _bodyMaxNum)
                        return true;
                    break;
                case ItemPart.Arm:
                    if (_arm.Count < _armMaxNum)
                        return true;
                    break;
                case ItemPart.Leg:
                    return true;
            }
            return false;
        }

        // 아이템 장착하기
        public void EquipItem(GameObject item)
        {
            Item ObjItem = item.GetComponent<Item>();
            switch (ObjItem.ItemPart)
            {
                case ItemPart.Head:
                    _head.Add(item);
                    break;
                case ItemPart.Body:
                    _body.Add(item);
                    break;
                case ItemPart.Arm:
                    _arm.Add(item);
                    break;
                case ItemPart.Leg:
                    _leg.Add(item);
                    break;

            }
        }

        // 아이템 버리기
        public void DropItem(ItemPart itemPart, int index)
        {
            switch (itemPart)
            {
                case ItemPart.Head:
                    _head.RemoveAt(index);
                    break;
                case ItemPart.Body:
                    _body.RemoveAt(index);
                    break;
                case ItemPart.Arm:
                    _arm.RemoveAt(index);
                    break;
                case ItemPart.Leg:
                    _leg.RemoveAt(index);
                    break;

            }
        }
    }
}