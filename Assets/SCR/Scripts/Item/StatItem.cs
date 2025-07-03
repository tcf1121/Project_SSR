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

        [Header("강화시 스탯")]
        [SerializeField] private Stats _strengthening;
        public Stats Strengthening { get => _strengthening; }


        override protected void Init()
        {
            base.Init();
            _itemPart = ItemPart.Leg;
            _currentStat = _basicStat;
        }

        override public void ItemEnhancement()
        {
            base.ItemEnhancement();
            _currentStat.Enhancement(_strengthening);
        }

        public void Clone(StatItem statItem, bool isGameobject = true)
        {
            _image = statItem.Image;
            _itemPart = statItem.ItemPart;
            _code = statItem.Code;
            _name = statItem.Name;
            _description = statItem.Description;
            _unlockDescription = statItem.Description;
            _isUnlock = statItem.IsUnlock;
            itemPrefab = statItem.itemPrefab;
            _strengthening = statItem.Strengthening;
            _basicStat = statItem.BasicStat;
            _currentStat = statItem.BasicStat;
            _specialStat = statItem.SpecialStat;
            if (isGameobject)
            {
                gameObject.transform.localScale = statItem.gameObject.transform.localScale;
                gameObject.GetComponent<BoxCollider2D>().offset = statItem.gameObject.GetComponent<BoxCollider2D>().offset;
                gameObject.GetComponent<BoxCollider2D>().size = statItem.gameObject.GetComponent<BoxCollider2D>().size;
                gameObject.GetComponent<SpriteRenderer>().sprite = _image;
            }
        }
    }
}
