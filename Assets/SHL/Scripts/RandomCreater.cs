using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace SHL
{
    public class RandomCreater : MonoBehaviour
    {
        public float randompoint;
        public float mapsizex;
        public float mapsizey;
        public List<Vector2> transformList;
       // public GameObject[] transformList;
        bool chacker;
        [SerializeField] private GameObject Posprefab;

        private void Start()
        {
            chacker = false;
            transformList = new();

            //for (int i = 0; i < randompoint; i++)
            //{
            //}

                PointCreate();
            //int layerMask = LayerMask.GetMask("Ground");
            //Vector2 dir = new Vector2(transformList[0].x, -mapsizey);
            //RaycastHit2D hit = Physics2D.Raycast(transformList[0], Vector2.down, mapsizey ,layerMask);
            //chacker = hit;
            //Debug.Log(hit);

            //Debug.DrawLine(transformList[0], dir, Color.red, 10);

        }
        void PointCreate()
        {
            chacker = false;
            for (int i = 0; i < randompoint; i++)
            {
                //while (!chacker)
                //{
                    float divisionRange = mapsizex / randompoint;
                    float xpos = Random.Range(-mapsizex/2+(i * divisionRange), -mapsizex/2+(divisionRange * (i + 1)));
                    float ypos = Random.Range(-(mapsizey/2), mapsizey/2);
                Vector2 pos = new Vector2(xpos, ypos);
                transformList.Add(new Vector2(xpos, ypos));
                //임시 확인용
                GameObject posobj = Instantiate(Posprefab);
                posobj.transform.position = pos;

                Vector2 dir = new Vector2(0,-1);
                int layerMask = LayerMask.GetMask("Ground");
                RaycastHit2D hit = Physics2D.Raycast(transformList[i],Vector2.down , mapsizey ,layerMask);

                chacker = hit;
                Debug.Log($"{i},{chacker}");
                Debug.DrawRay(transformList[i], Vector2.down*mapsizey, Color.red,100f);


                //}
            }


        }
       

    }
}

