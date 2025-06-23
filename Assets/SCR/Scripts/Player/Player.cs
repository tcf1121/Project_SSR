using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCR
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private GameObject _pickTrigger;
        [SerializeField] private Equipped equipped;
        [SerializeField] private ItemInfoUI itemInfoUI;

        private Rigidbody2D _rigid;
        Coroutine pickCor;

        void Awake()
        {
            _rigid = GetComponent<Rigidbody2D>();
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
            if (equipped.CheckItem(item))
            {
                equipped.EquipItem(item);
                item.SetActive(false);
                itemInfoUI.SetItem(item);
                itemInfoUI.gameObject.SetActive(true);
            }

        }

        IEnumerator pickup()
        {
            _pickTrigger.SetActive(true);
            Debug.Log("줍기");
            yield return new WaitForSeconds(1.0f);
            _pickTrigger.SetActive(false);
            StopCoroutine(pickCor);
            pickCor = null;
        }
    }
}

