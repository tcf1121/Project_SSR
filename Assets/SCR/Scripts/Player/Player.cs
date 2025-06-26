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
        public ConditionalUI ConditionalUI { get { return _conditionalUI; } }
        [SerializeField] private ConditionalUI _conditionalUI;
        public AlwaysOnUI AlwaysOnUI { get { return _alwaysOnUI; } }
        [SerializeField] private AlwaysOnUI _alwaysOnUI;
        public GameObject WaitItem { get { return _waitItem; } }
        private GameObject _waitItem;

        private Rigidbody2D _rigid;
        Coroutine pickCor;

        void Awake()
        {
            _rigid = GetComponent<Rigidbody2D>();
            _alwaysOnUI.LinkedPlayer(this);
            _conditionalUI.LinkedPlayer(this);
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
                _conditionalUI.EquipUI.OnOffEquipUI();
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
                    _conditionalUI.ItemInfoUI.SetItem(item);
                    _conditionalUI.ItemInfoUI.GetItem();
                }
                else
                {
                    //교체하기
                    _waitItem = item;
                    _conditionalUI.EquipUI.gameObject.SetActive(true);
                    _conditionalUI.EquipUI.IsChangeEquip(true);
                    _conditionalUI.EquipUI.OpenEquip((int)item.GetComponent<Item>().ItemPart);
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

