using System.Collections.Generic;
using UnityEngine;

namespace SCR
{
    public class Equipped : MonoBehaviour
    {
        private int _maxNum;

        public List<Item> Head { get { return _head; } }
        private List<Item> _head;

        public List<Item> Arm { get { return _arm; } }
        private List<Item> _arm;

        public List<Item> Foot { get { return _foot; } }
        private List<Item> _foot;

        public List<Item> Body { get { return _body; } }
        private List<Item> _body;

        private void Awake() => Init();

        private void Init()
        {
            _head = new();
            _arm = new();
            _foot = new();
            _body = new();
            _maxNum = 3;
        }


        // 아이템 꽉 차 있는지 확인
        public bool CheckItem(Item item)
        {
            switch (item.ItemPart)
            {
                case ItemPart.Head:
                    if (_head.Count < _maxNum)
                        return true;
                    break;
                case ItemPart.Arm:
                    if (_arm.Count < _maxNum)
                        return true;
                    break;
                case ItemPart.Foot:
                    if (_foot.Count < _maxNum)
                        return true;
                    break;
                case ItemPart.Body:
                    if (_body.Count < _maxNum)
                        return true;
                    break;
            }
            return false;
        }

        // 아이템 장착하기
        public void EquipItem(Item item)
        {
            switch (item.ItemPart)
            {
                case ItemPart.Head:
                    _head.Add(item);
                    break;
                case ItemPart.Arm:
                    _arm.Add(item);
                    break;
                case ItemPart.Foot:
                    _foot.Add(item);
                    break;
                case ItemPart.Body:
                    _body.Add(item);
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
                case ItemPart.Arm:
                    _arm.RemoveAt(index);
                    break;
                case ItemPart.Foot:
                    _foot.RemoveAt(index);
                    break;
                case ItemPart.Body:
                    _body.RemoveAt(index);
                    break;
            }
        }
    }
}