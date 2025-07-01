using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace SCR
{
    public class Equipped : MonoBehaviour
    {
        private Player player;
        public int HeadMaxNum { get { return _headMaxNum; } }
        private int _headMaxNum;
        public int BodyMaxNum { get { return _bodyMaxNum; } }
        private int _bodyMaxNum;
        public int ArmMaxNum { get { return _armMaxNum; } }
        private int _armMaxNum;

        public List<AttackItem> Head { get { return _head; } }
        [SerializeField] private List<AttackItem> _head;

        public List<AttackItem> Body { get { return _body; } }
        [SerializeField] private List<AttackItem> _body;

        public List<AttackItem> Arm { get { return _arm; } }
        [SerializeField] private List<AttackItem> _arm;

        public List<StatItem> Leg { get { return _leg; } }
        [SerializeField] private List<StatItem> _leg;

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
            GameManager.SelectEvent += DropItem;
        }

        // 특정 아이템이 있는지 확인
        public bool CheckItem(GameObject item)
        {
            Item ObjItem = item.GetComponent<Item>();
            int find = 0;

            switch (ObjItem.ItemPart)
            {

                case ItemPart.Head:
                    find = _head.FindIndex(n => n.GetComponent<AttackItem>().Name == ObjItem.Name);
                    if (find != -1)
                    {
                        EnhancementItem(ItemPart.Head, find);
                        return true;
                    }
                    break;
                case ItemPart.Body:
                    find = _body.FindIndex(n => n.GetComponent<AttackItem>().Name == ObjItem.Name);
                    if (find != -1)
                    {
                        EnhancementItem(ItemPart.Body, find);
                        return true;
                    }
                    break;
                case ItemPart.Arm:
                    find = _arm.FindIndex(n => n.GetComponent<AttackItem>().Name == ObjItem.Name);
                    if (find != -1)
                    {
                        EnhancementItem(ItemPart.Arm, find);
                        return true;
                    }
                    break;
                case ItemPart.Leg:
                    find = _leg.FindIndex(n => n.GetComponent<StatItem>().Name == ObjItem.Name);
                    if (find != -1)
                    {
                        EnhancementItem(ItemPart.Leg, find);
                        return true;
                    }
                    break;
            }
            return false;
        }

        // 아이템 꽉 차 있는지 확인
        public bool CheckFull(GameObject item)
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
                    _head.Add(item.GetComponent<AttackItem>());
                    break;
                case ItemPart.Body:
                    _body.Add(item.GetComponent<AttackItem>());
                    break;
                case ItemPart.Arm:
                    _arm.Add(item.GetComponent<AttackItem>());
                    break;
                case ItemPart.Leg:
                    _leg.Add(item.GetComponent<StatItem>());
                    break;

            }
        }

        // 아이템 강화하기
        public void EnhancementItem(ItemPart itemPart, int index)
        {
            switch (itemPart)
            {
                case ItemPart.Head:
                    _head[index].ItemEnhancement();
                    player.PlayerWeapon.HeadWeapons[index].Enhancement();
                    break;
                case ItemPart.Body:
                    _body[index].ItemEnhancement();
                    player.PlayerWeapon.BodyWeapons[index].Enhancement();
                    break;
                case ItemPart.Arm:
                    _arm[index].ItemEnhancement();
                    player.PlayerWeapon.ArmWeapons[index].Enhancement();
                    break;
                case ItemPart.Leg:
                    _leg[index].ItemEnhancement();
                    break;
            }
        }

        // 아이템 버리기
        public void DropItem(ItemPart itemPart, int index)
        {
            GameManager.StageManager.ItemSpawner.SetPos(player.gameObject.transform.position);
            GameObject DropObj = new();
            switch (itemPart)
            {
                case ItemPart.Head:
                    GameManager.StageManager.ItemSpawner.Spawn(_head[index].itemPrefab);
                    _head.RemoveAt(index);
                    Destroy(player.PlayerWeapon.HeadWeapons[index].gameObject);
                    player.PlayerWeapon.HeadWeapons.RemoveAt(index);
                    _head.Add(player.WaitItem.GetComponent<AttackItem>());
                    break;
                case ItemPart.Body:
                    GameManager.StageManager.ItemSpawner.Spawn(_body[index].itemPrefab);
                    _body.RemoveAt(index);
                    Destroy(player.PlayerWeapon.BodyWeapons[index].gameObject);
                    player.PlayerWeapon.BodyWeapons.RemoveAt(index);
                    _body.Add(player.WaitItem.GetComponent<AttackItem>());
                    break;
                case ItemPart.Arm:
                    GameManager.StageManager.ItemSpawner.Spawn(_arm[index].itemPrefab);
                    _arm.RemoveAt(index);
                    Destroy(player.PlayerWeapon.ArmWeapons[index].gameObject);
                    player.PlayerWeapon.ArmWeapons.RemoveAt(index);
                    _arm.Add(player.WaitItem.GetComponent<AttackItem>());
                    break;
                case ItemPart.Leg:
                    break;
            }
            DropObj.transform.position = player.transform.position;
            player.ConditionalUI.EquipUI.gameObject.SetActive(false);
        }
    }
}