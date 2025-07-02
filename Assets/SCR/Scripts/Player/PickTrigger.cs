using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

namespace SCR
{
    public class PickTrigger : MonoBehaviour
    {
        Player _player;
        [SerializeField] BoxCollider2D _collider;
        [SerializeField] private List<GameObject> itemObject;

        void Awake()
        {
            _player = GetComponentInParent<Player>();
            itemObject = new();
            _collider.enabled = false;
        }

        void OnEnable()
        {
            _collider.enabled = true;
        }

        void OnDisable()
        {
            _collider.enabled = false;
            if (itemObject.Count != 0)
            {
                _player.Equip(CheckDistance());
                itemObject.Clear();
            }
        }

        void OnTriggerEnter2D(Collider2D collision)
        {

            if (collision.gameObject.CompareTag("Teleport"))
            {
                _collider.enabled = false;
                return;
            }
            else if (collision.gameObject.CompareTag("Box"))
            {
                _collider.enabled = false;
                // 상자 가격은 시간이 지날수록 오르고 그거보다 많으면 플레이어 코인 감소하면서
                collision.gameObject.GetComponent<Box>().BoxOpen();
                return;
            }
            else if (collision.gameObject.CompareTag("Item"))
            {
                _collider.enabled = false;
                itemObject.Add(collision.gameObject);
                return;
            }
        }

        public GameObject CheckDistance()
        {
            if (itemObject.Count != 0)
            {
                GameObject shortItem = itemObject[0];
                float shortdis = Vector2.Distance(gameObject.transform.position,
                itemObject[0].transform.position);
                foreach (GameObject itemObj in itemObject)
                {
                    if (shortdis > Vector2.Distance(gameObject.transform.position,
                        itemObj.transform.position))
                    {
                        shortdis = Vector2.Distance(gameObject.transform.position,
                    itemObj.transform.position);
                        shortItem = itemObj;
                    }

                }
                return shortItem;
            }
            return null;
        }
    }
}
