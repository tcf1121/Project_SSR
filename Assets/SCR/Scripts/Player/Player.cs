using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SCR
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private GameObject _pickTrigger;
        public Equipped Equipped { get { return _equipped; } }
        [SerializeField] private Equipped _equipped;
        public ItemInfoUI ItemInfoUI { get { return _itemInfoUI; } }
        [SerializeField] private ItemInfoUI _itemInfoUI;
        public EquipUI EquipUI { get { return EquipUI; } }
        [SerializeField] private EquipUI _equipUI;
        public GameObject WaitItem { get { return _waitItem; } }
        private GameObject _waitItem;

        private Rigidbody2D _rigid;
        Coroutine pickCor;

        void Awake()
        {
            _rigid = GetComponent<Rigidbody2D>();
            _equipUI.SetEquipped(this);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (pickCor == null)
                {
                    pickCor = StartCoroutine(pickup());
                }
            }
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (_equipUI.gameObject.activeSelf)
                {
                    _equipUI.gameObject.SetActive(false);
                    Time.timeScale = 1f;
                }
                else
                {
                    _equipUI.gameObject.SetActive(true);
                    _equipUI.IsChangeEquip(false);
                    Time.timeScale = 0f;
                }
            }
            Move();
        }

        private void Move()
        {
            Vector3 moveVelocity = Vector3.zero;

            if (Input.GetAxisRaw("Horizontal") < 0)
            {
                moveVelocity = Vector3.left;
            }

            else if (Input.GetAxisRaw("Horizontal") > 0)
            {
                moveVelocity = Vector3.right;
            }

            transform.position += moveVelocity * 5f * Time.deltaTime;
        }

        public void Equip(GameObject item)
        {
            if (_equipped.CheckItem(item))
            {
                item.SetActive(false);
            }
            else
            {
                if (_equipped.CheckFull(item))
                {
                    _equipped.EquipItem(item);
                    item.SetActive(false);
                    _itemInfoUI.SetItem(item);
                    _itemInfoUI.GetItem();
                }
                else
                {
                    //교체하기
                    _waitItem = item;
                    _equipUI.gameObject.SetActive(true);
                    _equipUI.IsChangeEquip(true);
                    _equipUI.OpenEquip((int)item.GetComponent<Item>().ItemPart);
                    Time.timeScale = 0f;

                }
            }
        }

        IEnumerator pickup()
        {
            _pickTrigger.SetActive(true);
            yield return new WaitForSeconds(1.0f);
            _pickTrigger.SetActive(false);
            StopCoroutine(pickCor);
            pickCor = null;
        }
    }
}

