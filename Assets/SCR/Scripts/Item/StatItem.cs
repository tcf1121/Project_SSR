using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCR
{
    public class StatItem : Item
    {
        [Header("기본 스탯")]
        [SerializeField] private Stats _basicStat;
        public Stats BasicStat { get { return _basicStat; } }


        [Header("현재 스탯")]
        [SerializeField] private Stats _currentStat;
        public Stats CurrentStat { get { return _currentStat; } }

        // 특수 스탯
        public float SpecialStat { get { return _specialStat; } }
        private float _specialStat;

        // 강화 비율
        [SerializeField] private float _strengthening;

        override protected void Init()
        {
            base.Init();
            _itemPart = ItemPart.Leg;
        }

        override public void ItemEnhancement()
        {
            base.ItemEnhancement();
            _currentStat.Enhancement(_basicStat, _strengthening * _enhance);
        }
    }
}
