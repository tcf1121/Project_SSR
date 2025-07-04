using System.Collections;
using System.Collections.Generic;
using SCR;
using TMPro;
using UnityEngine;

namespace SCR
{
    public class ConditionalUI : MonoBehaviour
    {
        public ItemInfoUI ItemInfoUI { get { return _itemInfoUI; } }
        [SerializeField] private ItemInfoUI _itemInfoUI;
        public EnhancementInfo EnhancementInfo { get { return _enhancementInfo; } }
        [SerializeField] private EnhancementInfo _enhancementInfo;
        [SerializeField] private TMP_Text _qusetText;
        [SerializeField] private TMP_Text _guideText;

        [Header("EquipUI")]
        [SerializeField] private EquipUI _equipUI;
        public EquipUI EquipUI { get { return _equipUI; } }
        [SerializeField] private ChangeEquipUI _changeEquipUI;
        public ChangeEquipUI ChangeEquipUI { get { return _changeEquipUI; } }

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
            _changeEquipUI.SetEquipped(player);
        }
    }
}

