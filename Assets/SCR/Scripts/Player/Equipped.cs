using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Utill;

namespace SCR
{
    public class Equipped : MonoBehaviour
    {
        private Player player;
        public int HeadMaxNum { get { return _headMaxNum; } }
        private int _headMaxNum;
        private int _headFullMaxNum;
        public int BodyMaxNum { get { return _bodyMaxNum; } }
        private int _bodyMaxNum;
        private int _bodyFullMaxNum;
        public int ArmMaxNum { get { return _armMaxNum; } }
        private int _armMaxNum;
        private int _armFullMaxNum;

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
            _headFullMaxNum = 3;
            _bodyFullMaxNum = 5;
            _armFullMaxNum = 7;
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
            ItemPart itemPart = item.GetComponent<Item>().ItemPart;
            switch (item.GetComponent<Item>().ItemPart)
            {
                case ItemPart.Head:
                case ItemPart.Body:
                case ItemPart.Arm:
                    GameObject attackItem = ObjectPool.TakeFromPool(EPoolObjectType.EquipAItem);
                    attackItem.GetComponent<AttackItem>().Clone(item.GetComponent<AttackItem>());
                    attackItem.SetActive(false);
                    if (itemPart == ItemPart.Head)
                        _head.Add(attackItem.GetComponent<AttackItem>());
                    else if (itemPart == ItemPart.Body)
                        _body.Add(attackItem.GetComponent<AttackItem>());
                    else
                        _arm.Add(attackItem.GetComponent<AttackItem>());
                    break;
                case ItemPart.Leg:
                    GameObject statItem = ObjectPool.TakeFromPool(EPoolObjectType.EquipSItem);
                    statItem.GetComponent<StatItem>().Clone(item.GetComponent<StatItem>());
                    statItem.SetActive(false);
                    _leg.Add(statItem.GetComponent<StatItem>());
                    GetStat();
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
                    GetStat();
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
                    ObjectPool.ReturnPool(_head[index].gameObject, EPoolObjectType.EquipAItem);
                    _head.RemoveAt(index);
                    Destroy(player.PlayerWeapon.HeadWeapons[index].gameObject);
                    player.PlayerWeapon.HeadWeapons.RemoveAt(index);
                    break;
                case ItemPart.Body:
                    GameManager.StageManager.ItemSpawner.Spawn(_body[index].itemPrefab);
                    ObjectPool.ReturnPool(_body[index].gameObject, EPoolObjectType.EquipAItem);
                    _body.RemoveAt(index);
                    Destroy(player.PlayerWeapon.BodyWeapons[index].gameObject);
                    player.PlayerWeapon.BodyWeapons.RemoveAt(index);
                    break;
                case ItemPart.Arm:
                    GameManager.StageManager.ItemSpawner.Spawn(_arm[index].itemPrefab);
                    ObjectPool.ReturnPool(_arm[index].gameObject, EPoolObjectType.EquipAItem);
                    _arm.RemoveAt(index);
                    Destroy(player.PlayerWeapon.ArmWeapons[index].gameObject);
                    player.PlayerWeapon.ArmWeapons.RemoveAt(index);
                    break;
                case ItemPart.Leg:
                    //ObjectPool.ReturnPool(_leg[index].gameObject, EPoolObjectType.EquipSItem);
                    break;
            }
            player.ConditionalUI.ChangeEquipUI.EndChange(true);
        }

        public void GetStat()
        {
            Stats stats = new();
            stats.ResetStats();
            foreach (StatItem statItem in _leg)
                stats.AddStats(statItem.CurrentStat);
            player.PlayerStats.EquipItem(stats);
        }

        public bool CheckSlot()
        {
            // 모두 최대치로 해금했으면 취소
            if (_headMaxNum == _headFullMaxNum &&
            _bodyMaxNum == _bodyFullMaxNum &&
            _armMaxNum == _armFullMaxNum)
                return false;
            else return true;
        }

        public void ExpansionSlot()
        {
            int random;
            while (true)
            {
                random = Random.Range(0, 100);
                if (random < 25)
                {
                    if (_headMaxNum < _headFullMaxNum)
                    {
                        _headMaxNum++;
                        break;
                    }
                }
                else if (random < 55)
                {
                    if (_bodyMaxNum < _bodyFullMaxNum)
                    {
                        _bodyMaxNum++;
                        break;
                    }
                }
                else
                {
                    if (_armMaxNum < _armFullMaxNum)
                    {
                        _armMaxNum++;
                        break;
                    }
                }

            }
        }
    }
}