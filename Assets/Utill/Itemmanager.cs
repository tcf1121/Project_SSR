using SCR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SHL;
namespace Utill
{

    public class Itemmanager : MonoBehaviour
    {
        public enum ItemType { Head,Body,Arm,Leg}
        private ItemType type;
        public static Itemmanager instance;
        [SerializeField] private List<Item> itemType;
        private List<Item> itemCreate;
        private List<Vector2> spwanPoint;
        private SHL.Box box;
        public ObjectPool ObjectPool { get { return _objectPool; } set { _objectPool = value; } }
        [SerializeField] private ObjectPool _objectPool;



        private void Start()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(this.gameObject);
            _objectPool = GetComponent<ObjectPool>();
            
            itemCreate = new List<Item>();
            spwanPoint = new List<Vector2>();
            box = new SHL.Box();
        }
        void itemcreate(ItemType type)
        {
            switch(type)
            {
                case ItemType.Head:
                    //List<GameObject> Head = _objectPool.overlappingPrefab.FindAll(x => x.GetComponent<Item>().ItemPart == ItemPart.Head);
                    List<Item> Head = itemType.FindAll(x => x.GetComponent<Item>().ItemPart == ItemPart.Head);
                    Item Headitem = Head[Random.Range(0, Head.Count)];
                    itemCreate.Add(Headitem);
                    spwanPoint.Add(box.transform.position);
                    foreach(GameObject item in _objectPool.Pool)
                    {
                        if(!item.activeSelf)
                        {
                            if (itemCreate.Count == 0)
                                break;
                            
                        }
                    }
                    
                    break;
                    case ItemType.Body:
                    //List<GameObject> Body = _objectPool.overlappingPrefab.FindAll(x => x.GetComponent<Item>().ItemPart == ItemPart.Body);
                    List<Item> Body = itemType.FindAll(x => x.GetComponent<Item>().ItemPart == ItemPart.Body);
                    Item Bodyitem = Body[Random.Range(0, Body.Count)];
                    itemCreate.Add(Bodyitem);
                    spwanPoint.Add(box.transform.position);
                    break;
                    case ItemType.Arm:
                    //List<GameObject> Arm = _objectPool.overlappingPrefab.FindAll(x => x.GetComponent<Item>().ItemPart == ItemPart.Arm);
                    List<Item> Arm = itemType.FindAll(x => x.GetComponent<Item>().ItemPart == ItemPart.Arm);
                    Item Armitem = Arm[Random.Range(0, Arm.Count)];
                    itemCreate.Add(Armitem);
                    spwanPoint.Add(box.transform.position);
                    break;
                    case ItemType.Leg:
                    //List<GameObject> Leg = _objectPool.overlappingPrefab.FindAll(x => x.GetComponent<Item>().ItemPart == ItemPart.Leg);
                    List<Item> Leg = itemType.FindAll(x => x.GetComponent<Item>().ItemPart == ItemPart.Leg);
                    Item legitem = Leg[Random.Range(0, Leg.Count)];
                   
                    itemCreate.Add(legitem);
                    spwanPoint.Add(box.transform.position);
                    break;
            }
        }
        /// <summary>
        /// 
        /// </summary>

        /// <param name="chance1"></param>각 확률을 적어야함
        /// <param name="chance2"></param> 
        /// <param name="chance3"></param> 
        /// <param name="chance4"></param>
        /// // 드랍 아이템 확률 머리아이템 10% 몸통 20% 팔 30% 다리 40%
       public  void Chance(float chance1, float chance2, float chance3, float chance4)
        {
            float Randomnumber = Random.Range(0f, 1f); // 0부터 1 사이의 랜덤 숫자를 생성
            if (Randomnumber < chance1)
            {
                //Debug.Log("Armitem");
                //int randomIndex = Random.Range(0, Armitem.Length);
                //Instantiate(Armitem[randomIndex], transform.position, Quaternion.identity);
               itemcreate(ItemType.Head);
                
            }
            else if (Randomnumber < chance2 + chance1)
            {
                //Debug.Log("Headitem");
                //int randomIndex = Random.Range(0, Headitem.Length);
                //Instantiate(Headitem[randomIndex], transform.position, Quaternion.identity);
                //int randomIndex = Random.Range(0, Itemmanager.instance.Headitem.Length);
                //GameObject Headitem = Itemmanager.instance.HeadItemCreate(randomIndex);
                itemcreate(ItemType.Body);
            }
            else if (Randomnumber < chance3 + chance2 + chance1)
            {
                //Debug.Log("Bodyitem");
                //int randomIndex = Random.Range(0, Bodyitem.Length);
                //Instantiate(Bodyitem[randomIndex], transform.position, Quaternion.identity);
                //int randomIndex = Random.Range(0, Itemmanager.instance.Bodyitem.Length);
                //GameObject Bodyitem = Itemmanager.instance.BodyItemCreate(randomIndex);
                itemcreate(ItemType.Arm);
            }
            else if (Randomnumber < chance4 + chance3 + chance2 + chance1)
            {
                //Debug.Log("Legitem");
                //int randomIndex = Random.Range(0, Legitem.Length);
                //Instantiate(Legitem[randomIndex], transform.position, Quaternion.identity);
                //int randomIndex = Random.Range(0, Itemmanager.instance.Legitem.Length);
                //GameObject Legitem = Itemmanager.instance.LegItemCreate(randomIndex);
                itemcreate(ItemType.Leg);
            }


        }
    }
}
