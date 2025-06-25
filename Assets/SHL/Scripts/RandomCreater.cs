using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace SHL
{
    public class RandomCreater : MonoBehaviour
    {
        public int testRandomPoint;
        public float mapsizex;
        public float mapsizey;
        public List<Vector2> transformList;
        // public GameObject[] transformList;
        bool chacker;
        private Vector2 min;
        private Vector2 max;
        [SerializeField] private GameObject Posprefab;

        private void Start()
        {
            chacker = false;
            transformList = new();

            //for (int i = 0; i < randompoint; i++)
            //{
            //}

            PointCreate(testRandomPoint);
            //int layerMask = LayerMask.GetMask("Ground");
            //Vector2 dir = new Vector2(transformList[0].x, -mapsizey);
            //RaycastHit2D hit = Physics2D.Raycast(transformList[0], Vector2.down, mapsizey ,layerMask);
            //chacker = hit;
            //Debug.Log(hit);

            //Debug.DrawLine(transformList[0], dir, Color.red, 10);

        }
        public void PointCreate(int randompoint)
        {
            chacker = false;
            for (int i = 0; i < randompoint; i++)
            {
                //while (!chacker)
                //{
                float divisionRange = mapsizex / (float)randompoint;

                min = new Vector2(-mapsizex / 2 + (i * divisionRange), -(mapsizey / 2));
                max = new Vector2(-mapsizex / 2 + (divisionRange * (i + 1)), mapsizey / 2);
                Vector2 pos = RandomPoint(min, max);
                transformList.Add(pos);

                //임시 확인용
                GameObject posobj = Instantiate(Posprefab);
                posobj.transform.position = pos;

                Vector2 dir = new Vector2(0, -1);
                int layerMask = LayerMask.GetMask("Ground");
                RaycastHit2D hit = Physics2D.Raycast(transformList[i], Vector2.down, mapsizey, layerMask);

                chacker = hit;
                Debug.Log($"{i},{chacker}");
                Debug.DrawRay(transformList[i], Vector2.down * mapsizey, Color.red, 100f);


                //}
            }
        }

        public Vector2 RandomPoint(Vector2 min, Vector2 max)
        {
            float xpos = Random.Range(min.x, max.x);
            float ypos = Random.Range(min.y, max.y);

            return new Vector2(xpos, ypos);
        }


    }
}

