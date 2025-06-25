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
                transformList.Add(new Vector2(0, 0)); //초기화용
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
                    //랜덤 포인트 계산식 사이즈 대입식. 
                    float divisionRange = mapsizex / (float)randompoint; //랜덤 포인트를 나누기 위한 범위
                    min = new Vector2(-mapsizex / 2 + (i * divisionRange), -(mapsizey / 2));
                    max = new Vector2(-mapsizex / 2 + (divisionRange * (i + 1)), mapsizey / 2);
                    Vector2 pos = PointSetting(min, max);
                    //float xpos = Random.Range(-mapsizex / 2 + (i * divisionRange), -mapsizex / 2 + (divisionRange * (i + 1)));
                    //float ypos = Random.Range(-(mapsizey / 2), mapsizey / 2);  //코드 변경.
                    //Vector2 pos = new Vector2(xpos, ypos); //임시 확인용 값.
                    //transformList.Add(new Vector2(xpos, ypos)); //버그 원인으로 사료됨 코드 변경.
                    transformList[i] = pos; //리스트에 위치값을 넣는다.
                    //임시 확인용
                    //GameObject posobj = Instantiate(Posprefab);
                    //posobj.transform.position = pos;


                    int layerMask = LayerMask.GetMask("Ground");
                    RaycastHit2D hit = Physics2D.Raycast(transformList[i], Vector2.down, mapsizey, layerMask);

                    //bool hitcheck = hit; //검사용 디버그.
                    //Debug.Log($"{i},{hitcheck}");  
                    //Debug.DrawRay(transformList[i], Vector2.down * mapsizey, Color.red, 100f);
                    // 바닥이 있는 것을 찾았고 바닥 위치로 위치 값을 변경한다.  검사 종료
                    if (hit)
                    {
                        hit.point = new Vector2(hit.point.x, hit.point.y + tilesize); //바닥 위치에 타일 사이즈를 더한다.
                        RaycastHit2D rematch = Physics2D.Raycast(hit.point, Vector2.down, mapsizey, layerMask);
                        //이후 정확히 바닥에 안착 시키기 위해 다시 반복한다.
                        //GameObject posobj = Instantiate(Posprefab); //테스트 종료로 인해 주석처리
                        //검사용 프리펩 포지션 생성
                      
                        //Vector2 hitdir = rematch.point; 필요없음.
                        //한번더 계산을 통해 완벽한 위치에 구현.
                        transformList[i] = rematch.point;
                        //posobj.transform.position = rematch.point; //임시    확인용 값. 테스트 종료로 인해 주석처리
                        chacker = hit;
                        //Debug($"Hit Position: {rematch.point}"); //임시 확인용 값. 테스트 종료로 인해 주석처리
                        Debug.DrawRay(rematch.point,Vector2.up*0.1f, Color.green, 100f); //스폰위치 체크용.
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


