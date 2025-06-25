using System.Collections.Generic;
using UnityEngine;
namespace SHL
{
    public class RandomCreater : MonoBehaviour
    {
        public int randompoint;
        public float mapsizex;
        public float mapsizey;
        public float tilesize;
        public List<Vector2> _transformList { get { return transformList; } }
        [SerializeField] private List<Vector2> transformList;
        // public GameObject[] transformList;
        bool chacker;
        [SerializeField] private GameObject Posprefab;
        private Vector2 min;
        private Vector2 max;

        private void Awake()
        {

            chacker = false;
            transformList = new();

            for (int i = 0; i < randompoint; i++)
            {
                transformList.Add(new Vector2(0, 0)); //�ʱ�ȭ��
            }

            PointCreate(randompoint);
            //int layerMask = LayerMask.GetMask("Ground");
            //Vector2 dir = new Vector2(transformList[0].x, -mapsizey);
            //RaycastHit2D hit = Physics2D.Raycast(transformList[0], Vector2.down, mapsizey ,layerMask);
            //chacker = hit;
            //Debug.Log(hit);

            //Debug.DrawLine(transformList[0], dir, Color.red, 10);

        }

        public void PointCreate(int randompoint)
        {

            for (int i = 0; i < randompoint; i++)
            {
                chacker = false;
                while (!chacker)
                {
                    //���� ����Ʈ ���� ������ ���Խ�. 
                    float divisionRange = mapsizex / (float)randompoint; //���� ����Ʈ�� ������ ���� ����
                    min = new Vector2(-mapsizex / 2 + (i * divisionRange), -(mapsizey / 2));
                    max = new Vector2(-mapsizex / 2 + (divisionRange * (i + 1)), mapsizey / 2);
                    Vector2 pos = PointSetting(min, max);
                    //float xpos = Random.Range(-mapsizex / 2 + (i * divisionRange), -mapsizex / 2 + (divisionRange * (i + 1)));
                    //float ypos = Random.Range(-(mapsizey / 2), mapsizey / 2);  //�ڵ� ����.
                    //Vector2 pos = new Vector2(xpos, ypos); //�ӽ� Ȯ�ο� ��.
                    //transformList.Add(new Vector2(xpos, ypos)); //���� �������� ���� �ڵ� ����.
                    transformList[i] = pos; //����Ʈ�� ��ġ���� �ִ´�.
                    //�ӽ� Ȯ�ο�
                    //GameObject posobj = Instantiate(Posprefab);
                    //posobj.transform.position = pos;


                    int layerMask = LayerMask.GetMask("Ground");
                    RaycastHit2D hit = Physics2D.Raycast(transformList[i], Vector2.down, mapsizey, layerMask);

                    //bool hitcheck = hit; //�˻�� �����.
                    //Debug.Log($"{i},{hitcheck}");  
                    //Debug.DrawRay(transformList[i], Vector2.down * mapsizey, Color.red, 100f);
                    // �ٴ��� �ִ� ���� ã�Ұ� �ٴ� ��ġ�� ��ġ ���� �����Ѵ�.  �˻� ����
                    if (hit)
                    {
                        hit.point = new Vector2(hit.point.x, hit.point.y + tilesize); //�ٴ� ��ġ�� Ÿ�� ����� ���Ѵ�.
                        RaycastHit2D rematch = Physics2D.Raycast(hit.point, Vector2.down, mapsizey, layerMask);
                        //���� ��Ȯ�� �ٴڿ� ���� ��Ű�� ���� �ٽ� �ݺ��Ѵ�.
                        //GameObject posobj = Instantiate(Posprefab); //�׽�Ʈ ����� ���� �ּ�ó��
                        //�˻�� ������ ������ ����
                      
                        //Vector2 hitdir = rematch.point; �ʿ����.
                        //�ѹ��� ����� ���� �Ϻ��� ��ġ�� ����.
                        transformList[i] = rematch.point;
                        //posobj.transform.position = rematch.point; //�ӽ�    Ȯ�ο� ��. �׽�Ʈ ����� ���� �ּ�ó��
                        chacker = hit;
                        //Debug($"Hit Position: {rematch.point}"); //�ӽ� Ȯ�ο� ��. �׽�Ʈ ����� ���� �ּ�ó��
                        Debug.DrawRay(rematch.point,Vector2.up*0.1f, Color.green, 100f); //������ġ üũ��.
                    }



                }
            }
        }
        

        Vector2 PointSetting(Vector2 min, Vector2 max)
        {
            float xpos = Random.Range(min.x, max.x);
            float ypos = Random.Range(min.y, max.y);
            //Vector2 pos = new Vector2(xpos, ypos); 

            return new Vector2(xpos, ypos);
        }

    }




}


