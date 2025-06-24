using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

namespace SCR
{
    public class PickTrigger : MonoBehaviour
    {
        Player player;
        [SerializeField] private List<GameObject> itemObject;

        void Awake()
        {
            player = GetComponentInParent<Player>();
            itemObject = new();
        }

        void OnDisable()
        {
            player.Equip(CheckDistase());
            itemObject.Clear();
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            // if (collision.gameObject.CompareTag("Tele"))
            // {
            //     return;
            // }
            // if (collision.gameObject.CompareTag("Box"))
            // {
            //     return;
            // }
            if (collision.gameObject.CompareTag("Item"))
            {
                itemObject.Add(collision.gameObject);
                return;
            }
        }

        public GameObject CheckDistase()
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
