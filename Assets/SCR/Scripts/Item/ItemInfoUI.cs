using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SCR
{
    public class ItemInfoUI : MonoBehaviour
    {
        public Item ItemInfo;
        [SerializeField] private Image itemImage;
        [SerializeField] private TMP_Text nameTxt;
        [SerializeField] private TMP_Text DescriptionTxt;
        private Transform basePos;
        Coroutine DisableCor;

        private void Awake()
        {
            basePos = transform;
            gameObject.SetActive(false);
        }


        private void OnEnable()
        {

        }

        private void OnDisable()
        {
            //ItemInfo = null;

        }

        public void GetItem()
        {
            transform.position = basePos.position;
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
            DescriptionTxt.text = ItemInfo.Description;
        }

        public void SetItem(Item ItemInfo)
        {
            itemImage.sprite = ItemInfo.Image;
            nameTxt.text = ItemInfo.Name;
            if (ItemInfo.Enhance > 0)
                nameTxt.text = $"{ItemInfo.Name} {ItemInfo.Enhance}+";
            DescriptionTxt.text = ItemInfo.Description;
        }

        IEnumerator DisableItemInfo()
        {
            yield return new WaitForSeconds(1.0f);
            gameObject.SetActive(false);
        }
    }
}

