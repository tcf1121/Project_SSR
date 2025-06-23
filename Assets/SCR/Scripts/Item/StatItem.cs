using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCR
{
    public class StatItem : Item
    {
        // 기본 스탯
        [SerializeField] private float _basicStat;

        // 현재 스탯
        public float CurrentStat { get { return _currentStat; } }
        private float _currentStat;

        // 강화 비율
        [SerializeField] private float _strengthening;

        override protected void Init()
        {
            base.Init();
            _itemPart = ItemPart.Leg;
        }

        override protected void ItemEnhancement()
        {
            base.ItemEnhancement();
            _currentStat = _basicStat + (int)(_basicStat * _strengthening);
        }
    }
}
