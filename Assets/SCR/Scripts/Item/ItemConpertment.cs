using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SCR
{
    public class ItemConpartment : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public GameObject Icon;
        public Image ItemImage;
        private Item _item;
        public int at;
        private float interval = 0.25f;
        private float doubleClickedTime = -1.0f;
        private bool IsDoubleClicked = false;
        public GameObject InfoUI;
        public ItemInfoUI Info;
        public bool IsChange { get { return _isChange; } }
        private bool _isChange;

        void Awake()
        {
            ItemImage = Icon.GetComponent<Image>();

        }

        void OnDisable()
        {
            if (InfoUI != null) InfoUI.SetActive(false);
        }

        public void Init(GameObject infoUI)
        {
            InfoUI = infoUI;
            Info = InfoUI.GetComponent<ItemInfoUI>();
        }

        public void SetItem(Item item)
        {
            _item = item;
            if (item == null) ItemImage.sprite = null;
            else ItemImage.sprite = _item.Image;
        }

        public void SetChange(bool isChange)
        {
            _isChange = isChange;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isChange)
            {
                GameManager.SelectEvent?.Invoke(_item.ItemPart, at);
            }
            else
            {
                IsDoubleClicked = false;
                doubleClickedTime = Time.time;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_item != null)
            {
                Info.SetItem(_item);
                InfoUI.SetActive(true);
                InfoUI.transform.position = new Vector2(transform.position.x + 50, transform.position.y + 50);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            InfoUI.SetActive(false);
        }
    }

}
