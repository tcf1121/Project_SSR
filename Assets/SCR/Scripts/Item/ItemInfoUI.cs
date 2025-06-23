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
        [SerializeField] private Text nameTxt;
        [SerializeField] private Text DescriptionTxt;
        Coroutine DisableCor;


        private void OnEnable()
        {
            StartCoroutine(DisableItemInfo());
        }

        private void OnDisable()
        {
            ItemInfo = null;
        }

        public void SetItem(GameObject item)
        {

            ItemInfo = item.GetComponent<Item>();
            itemImage.sprite = ItemInfo.Image;
            nameTxt.text = ItemInfo.Name;
            DescriptionTxt.text = ItemInfo.Description;
        }

        IEnumerator DisableItemInfo()
        {
            yield return new WaitForSeconds(1.0f);
            gameObject.SetActive(false);
        }
    }
}

