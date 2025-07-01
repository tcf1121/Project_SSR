using SCR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SHL;
using TMPro.Examples;
namespace Utill
{

    public class Itemmanager : MonoBehaviour
    {
        
        public static Itemmanager instance;
        [SerializeField] private List<Item> itemType;
        
        [SerializeField] private List<Vector2> spawnPoints;
        
        public ObjectPool ObjectPool { get { return _objectPool; } set { _objectPool = value; } }
        [SerializeField] private ObjectPool _objectPool;
        


        private void Start()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(this.gameObject);
            _objectPool = GetComponent<ObjectPool>();
            //GameObject[] boxes = GameObject.FindGameObjectsWithTag("Box");
            spawnPoints = SHL.BoxSetup.instance._spawnPoints;
            //spawnPoints = new List<Vector2>();
            //foreach(GameObject box in boxes)
            //{
            //    spawnPoints.Add(box.transform.position);
            //}
           
        }

        void spawn()
        {
            //itempart part = chance
            //if(itempart != itempart.leg)
            
        }
        void Itemcreate(ItemPart type)//리펙토링  게임오브젝트 반환.
        {
            List<Item> typefilter = itemType.FindAll(x => x.GetComponent<Item>().ItemPart == type);
            if (typefilter.Count == 0) return;

            Item select = typefilter[Random.Range(0, typefilter.Count)];
            foreach (GameObject item in _objectPool.Pool)
            {
                if (!item.activeSelf)
                {
                    if(item.GetComponent<Item>().ItemPart == type)
                    {
                        if (spawnPoints.Count == 0)
                            break;
                        item.transform.position = spawnPoints[0];
                        _objectPool.TakeFromPool(item);
                    }
                    spawnPoints.RemoveAt(0);


                }

            }
        }
        //void itemcreate(ItemType type) // 길어서 버림
        //{
        //    switch(type)
        //    {
        //        case ItemType.Head:
        //            //List<GameObject> Head = _objectPool.overlappingPrefab.FindAll(x => x.GetComponent<Item>().ItemPart == ItemPart.Head);
        //            List<Item> Head = itemType.FindAll(x => x.GetComponent<Item>().ItemPart == ItemPart.Head);
        //            Item Headitem = Head[Random.Range(0, Head.Count)];
        //            itemCreate.Add(Headitem);
        //            //spwanPoint.Add(box.transform.position);

        //            foreach(GameObject item in _objectPool.Pool)
        //            {
        //                if(!item.activeSelf)
        //                {
        //                    if (itemCreate.Count == 0)
        //                        break;
        //                    if(item.GetComponent<Item>().ItemPart == ItemPart.Head)
        //                    {
        //                        item.transform.position = GameObject.FindObjectsOfTypeAll

        //                    }

        //                }
        //            }

        //            break;
        //            case ItemType.Body:
        //            //List<GameObject> Body = _objectPool.overlappingPrefab.FindAll(x => x.GetComponent<Item>().ItemPart == ItemPart.Body);
        //            List<Item> Body = itemType.FindAll(x => x.GetComponent<Item>().ItemPart == ItemPart.Body);
        //            Item Bodyitem = Body[Random.Range(0, Body.Count)];
        //            itemCreate.Add(Bodyitem);
        //            //spwanPoint.Add(box.transform.position);
        //            break;
        //            case ItemType.Arm:
        //            //List<GameObject> Arm = _objectPool.overlappingPrefab.FindAll(x => x.GetComponent<Item>().ItemPart == ItemPart.Arm);
        //            List<Item> Arm = itemType.FindAll(x => x.GetComponent<Item>().ItemPart == ItemPart.Arm);
        //            Item Armitem = Arm[Random.Range(0, Arm.Count)];
        //            itemCreate.Add(Armitem);
        //            //spwanPoint.Add(box.transform.position);
        //            break;
        //            case ItemType.Leg:
        //            //List<GameObject> Leg = _objectPool.overlappingPrefab.FindAll(x => x.GetComponent<Item>().ItemPart == ItemPart.Leg);
        //            List<Item> Leg = itemType.FindAll(x => x.GetComponent<Item>().ItemPart == ItemPart.Leg);
        //            Item legitem = Leg[Random.Range(0, Leg.Count)];

        //            itemCreate.Add(legitem);
        //            //spwanPoint.Add(box.transform.position);
        //            break;
        //    }
        //}
        /// <summary>
        /// 
        /// </summary>

        /// <param name="chance1"></param>각 확률을 적어야함
        /// <param name="chance2"></param> 
        /// <param name="chance3"></param> 
        /// <param name="chance4"></param>
        /// // 드랍 아이템 확률 머리아이템 10% 몸통 20% 팔 30% 다리 40%
       public  ItemPart Chance(float chance1, float chance2, float chance3, float chance4)
        {
            float Randomnumber = Random.Range(0f, 1f); // 0부터 1 사이의 랜덤 숫자를 생성
            if (Randomnumber < chance1)
            {
                //Debug.Log("Armitem");
                //int randomIndex = Random.Range(0, Armitem.Length);
                //Instantiate(Armitem[randomIndex], transform.position, Quaternion.identity);
               //Itemcreate(ItemPart.Arm);
                return ItemPart.Arm;

            }
            else if (Randomnumber < chance2 + chance1)
            {
                //Debug.Log("Headitem");
                //int randomIndex = Random.Range(0, Headitem.Length);
                //Instantiate(Headitem[randomIndex], transform.position, Quaternion.identity);
                //int randomIndex = Random.Range(0, Itemmanager.instance.Headitem.Length);
                //GameObject Headitem = Itemmanager.instance.HeadItemCreate(randomIndex);
                //Itemcreate(ItemPart.Head);
                return ItemPart.Head;
            }
            else if (Randomnumber < chance3 + chance2 + chance1)
            {
                //Debug.Log("Bodyitem");
                //int randomIndex = Random.Range(0, Bodyitem.Length);
                //Instantiate(Bodyitem[randomIndex], transform.position, Quaternion.identity);
                //int randomIndex = Random.Range(0, Itemmanager.instance.Bodyitem.Length);
                //GameObject Bodyitem = Itemmanager.instance.BodyItemCreate(randomIndex);
                //Itemcreate(ItemPart.Body);
                return ItemPart.Body;
            }
            else
            {
                //Debug.Log("Legitem");
                //int randomIndex = Random.Range(0, Legitem.Length);
                //Instantiate(Legitem[randomIndex], transform.position, Quaternion.identity);
                //int randomIndex = Random.Range(0, Itemmanager.instance.Legitem.Length);
                //GameObject Legitem = Itemmanager.instance.LegItemCreate(randomIndex);
                //Itemcreate(ItemPart.Leg);
                return ItemPart.Leg;
            }


        }
    }
}
