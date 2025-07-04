using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utill;

namespace SCR
{
    public class ChangeEquipUI : MonoBehaviour
    {
        [SerializeField] private Player _player;
        [SerializeField] private GameObject Content;
        [SerializeField] private GameObject ItemConPrefab;
        [SerializeField] private List<ItemConpartment> itemList;
        private GameObject _waitItem;
        private Vector2 _spawnPoint;

        private void SetItemList()
        {
            for (int i = 0; i < 20; i++)
            {
                GameObject itemCon = Instantiate(ItemConPrefab);
                itemCon.transform.SetParent(Content.transform);
                itemList.Add(itemCon.GetComponent<ItemConpartment>());
                itemList[i].Init(_player.ConditionalUI.ItemInfoUI.gameObject);
                itemList[i].at = i;
            }
        }

        public void SetEquipped(Player player)
        {
            _player = player;
        }

        public void OpenEquip(int part)
        {
            if (itemList.Count == 0) SetItemList();
            foreach (ItemConpartment item in itemList)
            {
                item.gameObject.SetActive(false);
                item.SetChange(true);
                item.SetItem(null);
            }
            if (part == (int)ItemPart.Head)
            {
                for (int i = 0; i < _player.Equipped.HeadMaxNum; i++)
                {
                    if (i < _player.Equipped.Head.Count)
                        itemList[i].SetItem(_player.Equipped.Head[i]);
                    itemList[i].gameObject.SetActive(true);
                }
            }
            else if (part == (int)ItemPart.Body)
            {
                for (int i = 0; i < _player.Equipped.BodyMaxNum; i++)
                {
                    if (i < _player.Equipped.Body.Count)
                        itemList[i].SetItem(_player.Equipped.Body[i]);
                    itemList[i].gameObject.SetActive(true);
                }
            }
            else if (part == (int)ItemPart.Arm)
            {
                for (int i = 0; i < _player.Equipped.ArmMaxNum; i++)
                {
                    if (i < _player.Equipped.Arm.Count)
                        itemList[i].SetItem(_player.Equipped.Arm[i]);
                    itemList[i].gameObject.SetActive(true);
                }
            }
            else if (part == (int)ItemPart.Leg)
            {
                if (_player.Equipped.Leg.Count > 0)
                    for (int i = 0; i < _player.Equipped.Leg.Count; i++)
                    {
                        itemList[i].SetItem(_player.Equipped.Leg[i]);
                        itemList[i].gameObject.SetActive(true);
                    }
            }

        }

        public void StartChange(ItemPart part, GameObject waitItem)
        {
            _waitItem = waitItem;
            OpenEquip((int)part);
            gameObject.SetActive(true);
            Time.timeScale = 0f;
        }

        public void EndChange(bool isChange)
        {
            if (isChange)
            {
                _player.Equip(_waitItem);
            }
            else
            {
                GameObject item;
                if (_waitItem.GetComponent<AttackItem>() != null)
                {
                    item = ObjectPool.TakeFromPool(EPoolObjectType.AttackItem);
                    item.GetComponent<AttackItem>().Clone(_waitItem.GetComponent<AttackItem>());
                    item.transform.position = GameManager.Player.gameObject.transform.position;
                    item.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 2f, ForceMode2D.Impulse);
                }
                else
                {
                    item = ObjectPool.TakeFromPool(EPoolObjectType.StatItem);
                    item.GetComponent<StatItem>().Clone(_waitItem.GetComponent<StatItem>());
                    item.transform.position = GameManager.Player.gameObject.transform.position;
                    item.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 2f, ForceMode2D.Impulse);
                }
            }
            gameObject.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}

