
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
            
            // �� Ǯ�� ����(��Ȱ��ȭ��) �ִ� ���ӿ�����Ʈ ����

            //�߽߰� ������ ������ �Ҵ�
            foreach (GameObject item in Armpools[index])
            {
                if (!item.activeSelf)
                {
                    chioce = item;
                    chioce.SetActive(true);
                    break;
                    //�߽߰� ������ ������ �Ҵ�
                }
            }

            //�� ã������??

            if (!chioce)
            {
                chioce = Instantiate(Armitem[index], transform);
                Armpools[index].Add(chioce);
                //���Ӱ� �����ϰ� ������ ������ �Ҵ�
            }

            return chioce;
        }
        public GameObject HeadItemCreate(int index)
        {
            GameObject chioce = null;
            // �� Ǯ�� ����(��Ȱ��ȭ��) �ִ� ���ӿ�����Ʈ ����

            //�߽߰� ������ ������ �Ҵ�
            foreach (GameObject item in Headpools[index])
            {
                if (!item.activeSelf)
                {
                    chioce = item;
                    chioce.SetActive(true);
                    break;
                    //�߽߰� ������ ������ �Ҵ�
                }
            }

            //�� ã������??

            if (!chioce)
            {
                chioce = Instantiate(Headitem[index], transform);
                Headpools[index].Add(chioce);
                //���Ӱ� �����ϰ� ������ ������ �Ҵ�
            }

            return chioce;
        }
        public GameObject BodyItemCreate(int index)
        {
            GameObject chioce = null;
            // �� Ǯ�� ����(��Ȱ��ȭ��) �ִ� ���ӿ�����Ʈ ����

            //�߽߰� ������ ������ �Ҵ�
            foreach (GameObject item in Bodypools[index])
            {
                if (!item.activeSelf)
                {
                    chioce = item;
                    chioce.SetActive(true);
                    break;
                    //�߽߰� ������ ������ �Ҵ�
                }
            }

            //�� ã������??

            if (!chioce)
            {
                chioce = Instantiate(Bodyitem[index], transform);
                Bodypools[index].Add(chioce);
                //���Ӱ� �����ϰ� ������ ������ �Ҵ�
            }

            return chioce;
        }
        public GameObject LegItemCreate(int index)
        {
            GameObject chioce = null;
            // �� Ǯ�� ����(��Ȱ��ȭ��) �ִ� ���ӿ�����Ʈ ����

            //�߽߰� ������ ������ �Ҵ�
            foreach (GameObject item in Legpools[index])
            {
                if (!item.activeSelf)
                {
                    chioce = item;
                    chioce.SetActive(true);
                    break;
                    //�߽߰� ������ ������ �Ҵ�
                }
            }

            //�� ã������??

            if (!chioce)
            {
                chioce = Instantiate(Legitem[index], transform);
                Legpools[index].Add(chioce);
                //���Ӱ� �����ϰ� ������ ������ �Ҵ�
            }

            return chioce;
        }
        
       



    }
}