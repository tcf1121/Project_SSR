
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Utill
{
    public class X_Itemmanager : MonoBehaviour
    {
        //public static Itemmanager instance;
        [SerializeField]public GameObject[] Armitem;
        [SerializeField]public GameObject[] Headitem;
        [SerializeField]public GameObject[] Bodyitem;
        [SerializeField]public GameObject[] Legitem;
        List<GameObject>[] Armpools;
        List<GameObject>[] Headpools;
        List<GameObject>[] Bodypools;
        List<GameObject>[] Legpools;
        


        private void Awake()
        {
            
            //if (instance == null)
            //{
            //    instance = this;
            //}
            //else
            //{
            //    Destroy(this.gameObject);
            //}
            //init();
            
        }
        void init()
        {
            Armpools = new List<GameObject>[Armitem.Length];

            for (int i = 0; i < Armitem.Length; i++)
            {
                Armpools[i] = new List<GameObject>();
            }

            Headpools = new List<GameObject>[Headitem.Length];

            for (int i = 0; i < Headitem.Length; i++)
            {
                Headpools[i] = new List<GameObject>();
            }
            Bodypools = new List<GameObject>[Bodyitem.Length];

            for (int i = 0; i < Bodyitem.Length; i++)
            {
                Bodypools[i] = new List<GameObject>();
            }

            Legpools = new List<GameObject>[Legitem.Length];

            for(int i = 0;i < Legitem.Length;i++)
            {
                Legpools[i] = new List<GameObject>();
            }
        }
        public GameObject ArmItemCreate(int index)
        {
            GameObject chioce = null;
            
            // 고른 풀에 놓고(비활성화된) 있는 게임오브젝트 접근

            //발견시 선택한 변수에 할당
            foreach (GameObject item in Armpools[index])
            {
                if (!item.activeSelf)
                {
                    chioce = item;
                    chioce.SetActive(true);
                    break;
                    //발견시 선택한 변수에 할당
                }
            }

            //못 찾았으면??

            if (!chioce)
            {
                chioce = Instantiate(Armitem[index], transform);
                Armpools[index].Add(chioce);
                //새롭게 생성하고 선택한 변수에 할당
            }

            return chioce;
        }
        public GameObject HeadItemCreate(int index)
        {
            GameObject chioce = null;
            // 고른 풀에 놓고(비활성화된) 있는 게임오브젝트 접근

            //발견시 선택한 변수에 할당
            foreach (GameObject item in Headpools[index])
            {
                if (!item.activeSelf)
                {
                    chioce = item;
                    chioce.SetActive(true);
                    break;
                    //발견시 선택한 변수에 할당
                }
            }

            //못 찾았으면??

            if (!chioce)
            {
                chioce = Instantiate(Headitem[index], transform);
                Headpools[index].Add(chioce);
                //새롭게 생성하고 선택한 변수에 할당
            }

            return chioce;
        }
        public GameObject BodyItemCreate(int index)
        {
            GameObject chioce = null;
            // 고른 풀에 놓고(비활성화된) 있는 게임오브젝트 접근

            //발견시 선택한 변수에 할당
            foreach (GameObject item in Bodypools[index])
            {
                if (!item.activeSelf)
                {
                    chioce = item;
                    chioce.SetActive(true);
                    break;
                    //발견시 선택한 변수에 할당
                }
            }

            //못 찾았으면??

            if (!chioce)
            {
                chioce = Instantiate(Bodyitem[index], transform);
                Bodypools[index].Add(chioce);
                //새롭게 생성하고 선택한 변수에 할당
            }

            return chioce;
        }
        public GameObject LegItemCreate(int index)
        {
            GameObject chioce = null;
            // 고른 풀에 놓고(비활성화된) 있는 게임오브젝트 접근

            //발견시 선택한 변수에 할당
            foreach (GameObject item in Legpools[index])
            {
                if (!item.activeSelf)
                {
                    chioce = item;
                    chioce.SetActive(true);
                    break;
                    //발견시 선택한 변수에 할당
                }
            }

            //못 찾았으면??

            if (!chioce)
            {
                chioce = Instantiate(Legitem[index], transform);
                Legpools[index].Add(chioce);
                //새롭게 생성하고 선택한 변수에 할당
            }

            return chioce;
        }
        
       



    }
}