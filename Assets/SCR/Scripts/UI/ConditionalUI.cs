using System.Collections;
using System.Collections.Generic;
using SCR;
using TMPro;
using UnityEngine;

namespace SCR
{
    public class ConditionalUI : MonoBehaviour
    {
        [SerializeField] public ItemInfoUI ItemInfoUI { get { return _itemInfoUI; } }
        [SerializeField] private ItemInfoUI _itemInfoUI;
        [SerializeField] private TMP_Text _qusetText;
        [SerializeField] private TMP_Text _guideText;

        [Header("EquipUI")]
        [SerializeField] private EquipUI _equipUI;
        public EquipUI EquipUI { get { return _equipUI; } }

        public void SetQusetText(string text)
        {
            _qusetText.text = text;
        }

        public void SetGuideText(string text)
        {
            _guideText.text = text;
        }

        public void LinkedPlayer(Player player)
        {
            _equipUI.SetEquipped(player);
        }
    }
}

