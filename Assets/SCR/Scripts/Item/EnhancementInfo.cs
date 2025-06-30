using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SCR
{
    public class EnhancementInfo : MonoBehaviour
    {
        public Item ItemInfo;
        [SerializeField] private Image itemImage;
        [SerializeField] private TMP_Text nameTxt;
        [SerializeField] private Vector2 basePos;
        Coroutine DisableCor;

        private void Awake()
        {
            basePos = transform.position;
            gameObject.SetActive(false);
        }

        public void GetItem()
        {
            transform.position = basePos;
            gameObject.SetActive(true);
            StartCoroutine(DisableItemInfo());
        }

        public void SetItem(GameObject item)
        {
            ItemInfo = item.GetComponent<Item>();
            itemImage.sprite = ItemInfo.Image;
            nameTxt.text = ItemInfo.Name;
            if (ItemInfo.Enhance > 0)
                nameTxt.text = $"{ItemInfo.Name} {ItemInfo.Enhance}+";
        }

        public void SetItem(Item ItemInfo)
        {
            itemImage.sprite = ItemInfo.Image;
            nameTxt.text = ItemInfo.Name;
            if (ItemInfo.Enhance > 0)
                nameTxt.text = $"{ItemInfo.Name} {ItemInfo.Enhance}+";
        }

        IEnumerator DisableItemInfo()
        {
            yield return new WaitForSeconds(1.0f);
            gameObject.SetActive(false);
        }
    }
}

