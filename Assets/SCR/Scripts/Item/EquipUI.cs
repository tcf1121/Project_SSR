using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCR
{
    public class EquipUI : MonoBehaviour
    {
        private Player _player;
        [SerializeField] private GameObject equipPanel;
        [SerializeField] private GameObject Content;
        [SerializeField] private GameObject ItemConPrefab;
        private List<ItemConpartment> itemList;
        // Start is called before the first frame update

        void Awake() => Init();

        private void Init()
        {
            itemList = new();
            for (int i = 0; i < 20; i++)
            {
                GameObject itemCon = Instantiate(ItemConPrefab);
                itemCon.transform.SetParent(Content.transform);
                itemList.Add(itemCon.GetComponent<ItemConpartment>());
                itemList[i].Init(_player.ItemInfoUI.gameObject);
                itemList[i].at = i;
                itemList[i].gameObject.SetActive(false);
            }
        }

        void OnEnable()
        {
            equipPanel.SetActive(false);
        }

        public void SetEquipped(Player player)
        {
            _player = player;
        }

        public void IsChangeEquip(bool isChange)
        {
            foreach (ItemConpartment item in itemList)
            {
                item.SetChange(isChange);
            }
        }

        public void OpenEquip(int part)
        {
            foreach (ItemConpartment item in itemList)
            {
                item.gameObject.SetActive(false);
            }
            equipPanel.SetActive(true);
            if (part == (int)ItemPart.Head)
            {
                if (_player.Equipped.Head.Count > 0)
                    for (int i = 0; i < _player.Equipped.Head.Count; i++)
                    {
                        itemList[i].item = _player.Equipped.Head[i];
                        itemList[i].gameObject.SetActive(true);
                    }
            }
            else if (part == (int)ItemPart.Body)
            {
                if (_player.Equipped.Body.Count > 0)
                    for (int i = 0; i < _player.Equipped.Body.Count; i++)
                    {
                        itemList[i].item = _player.Equipped.Body[i];
                        itemList[i].gameObject.SetActive(true);
                    }
            }
            else if (part == (int)ItemPart.Arm)
            {
                if (_player.Equipped.Arm.Count > 0)
                    for (int i = 0; i < _player.Equipped.Arm.Count; i++)
                    {
                        itemList[i].item = _player.Equipped.Arm[i];
                        itemList[i].gameObject.SetActive(true);
                    }
            }
            else if (part == (int)ItemPart.Leg)
            {
                if (_player.Equipped.Leg.Count > 0)
                    for (int i = 0; i < _player.Equipped.Leg.Count; i++)
                    {
                        itemList[i].item = _player.Equipped.Leg[i];
                        itemList[i].gameObject.SetActive(true);
                    }
            }

        }
    }
}

