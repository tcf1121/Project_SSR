using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class EquipUI : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private Color _selectColor;
    [SerializeField] private Color _unselectColor;
    [SerializeField] private List<Toggle> _toggles;
    [SerializeField] private GameObject Content;
    [SerializeField] private GameObject ItemConPrefab;
    [SerializeField] private TMP_Text _addHp;
    [SerializeField] private TMP_Text _addRecoveryHp;
    [SerializeField] private TMP_Text _addDamage;
    [SerializeField] private TMP_Text _addSpeed;

    [SerializeField] private List<ItemConpartment> itemList;

    void Awake() => Init();

    private void Init()
    {
        //itemList = new();
        for (int i = 0; i < 20; i++)
        {
            GameObject itemCon = Instantiate(ItemConPrefab);
            itemCon.transform.SetParent(Content.transform);
            itemList.Add(itemCon.GetComponent<ItemConpartment>());
            itemList[i].Init(_player.ConditionalUI.ItemInfoUI.gameObject);
            itemList[i].at = i;
            itemList[i].gameObject.SetActive(false);
        }
    }

    void OnEnable()
    {
        OpenEquip(0);
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
        ChangeToggleColor();
        foreach (ItemConpartment item in itemList)
        {
            item.gameObject.SetActive(false);
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

    public void ChangeToggleColor()
    {
        for (int i = 0; i < _toggles.Count; i++)
        {
            ColorBlock colorBlock = _toggles[i].colors;
            bool isChoose = _toggles[i].isOn;
            // 선택 여부에 따라 색상 설정
            colorBlock.normalColor = isChoose ? _selectColor : _unselectColor;
            colorBlock.selectedColor = isChoose ? _selectColor : _unselectColor;
        }

    }

    public void SetAddAbility()
    {

    }

    public void OnOffEquipUI()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            Time.timeScale = 1f;
        }
        else
        {
            gameObject.SetActive(true);
            IsChangeEquip(false);
            Time.timeScale = 0f;
        }
    }
}

