using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace SCR
{
    public class Item : MonoBehaviour
    {
        // 아이템 이미지
        public Sprite Image { get { return _image; } }
        [SerializeField] private Sprite _image;

        // 아이템 부위
        public ItemPart ItemPart { get { return _itemPart; } }
        [SerializeField] protected ItemPart _itemPart;

        // 아이템 이름
        public string Name { get { return _name; } }
        [SerializeField] protected string _name;

        // 아이템 설명
        public string Description { get { return _description; } }
        [SerializeField] protected string _description;

        // 강화 단계
        public int Enhance { get { return _enhance; } }
        protected int _enhance;

        // 해금 방법
        public string UnlockDescription { get { return _unlockDescription; } }
        [SerializeField] protected string _unlockDescription;

        // 해금 상태
        public bool IsUnlock { get { return _isUnlock; } }
        [SerializeField] protected bool _isUnlock;

        public GameObject itemPrefab;


        private void Awake() => Init();

        virtual protected void Init()
        {
            GetComponent<SpriteRenderer>().sprite = _image;
            _enhance = 0;
        }

        public void Unlock()
        {
            _isUnlock = true;
        }

        virtual public void ItemEnhancement()
        {
            _enhance++;


        }

    }
    public enum ItemPart
    {
        Head,
        Body,
        Arm,
        Leg
    }
}

