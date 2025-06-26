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
        private Vector2 min;
        private Vector2 max;
        [SerializeField] private GameObject Posprefab;

        private void Awake()
        {

            chacker = false;
            transformList = new();

            for (int i = 0; i < randompoint; i++)
            {
                transformList.Add(new Vector2(0, 0)); //ÃÊ±âÈ­¿ë
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
                    //·£´ý Æ÷ÀÎÆ® °è»ê½Ä »çÀÌÁî ´ëÀÔ½Ä. 
                    float divisionRange = mapsizex / (float)randompoint; //·£´ý Æ÷ÀÎÆ®¸¦ ³ª´©±â À§ÇÑ ¹üÀ§
                    min = new Vector2(-mapsizex / 2 + (i * divisionRange), -(mapsizey / 2));
                    max = new Vector2(-mapsizex / 2 + (divisionRange * (i + 1)), mapsizey / 2);
                    Vector2 pos = PointSetting(min, max);
                    //float xpos = Random.Range(-mapsizex / 2 + (i * divisionRange), -mapsizex / 2 + (divisionRange * (i + 1)));
                    //float ypos = Random.Range(-(mapsizey / 2), mapsizey / 2);  //ÄÚµå º¯°æ.
                    //Vector2 pos = new Vector2(xpos, ypos); //ÀÓ½Ã È®ÀÎ¿ë °ª.
                    //transformList.Add(new Vector2(xpos, ypos)); //¹ö±× ¿øÀÎÀ¸·Î »ç·áµÊ ÄÚµå º¯°æ.
                    transformList[i] = pos; //¸®½ºÆ®¿¡ À§Ä¡°ªÀ» ³Ö´Â´Ù.
                    //ÀÓ½Ã È®ÀÎ¿ë
                    //GameObject posobj = Instantiate(Posprefab);
                    //posobj.transform.position = pos;

                    int layerMask = LayerMask.GetMask("Ground");
                    RaycastHit2D hit = Physics2D.Raycast(transformList[i], Vector2.down, mapsizey, layerMask);

                    //bool hitcheck = hit; //°Ë»ç¿ë µð¹ö±×.
                    //Debug.Log($"{i},{hitcheck}");  
                    //Debug.DrawRay(transformList[i], Vector2.down * mapsizey, Color.red, 100f);
                    // ¹Ù´ÚÀÌ ÀÖ´Â °ÍÀ» Ã£¾Ò°í ¹Ù´Ú À§Ä¡·Î À§Ä¡ °ªÀ» º¯°æÇÑ´Ù.  °Ë»ç Á¾·á
                    if (hit)
                    {
                        hit.point = new Vector2(hit.point.x, hit.point.y + tilesize); //¹Ù´Ú À§Ä¡¿¡ Å¸ÀÏ »çÀÌÁî¸¦ ´õÇÑ´Ù.
                        RaycastHit2D rematch = Physics2D.Raycast(hit.point, Vector2.down, mapsizey, layerMask);
                        //ÀÌÈÄ Á¤È®È÷ ¹Ù´Ú¿¡ ¾ÈÂø ½ÃÅ°±â À§ÇØ ´Ù½Ã ¹Ýº¹ÇÑ´Ù.
                        //GameObject posobj = Instantiate(Posprefab); //Å×½ºÆ® Á¾·á·Î ÀÎÇØ ÁÖ¼®Ã³¸®
                        //°Ë»ç¿ë ÇÁ¸®Æé Æ÷Áö¼Ç »ý¼º

                        //Vector2 hitdir = rematch.point; ÇÊ¿ä¾øÀ½.
                        //ÇÑ¹ø´õ °è»êÀ» ÅëÇØ ¿Ïº®ÇÑ À§Ä¡¿¡ ±¸Çö.
                        transformList[i] = rematch.point;
                        //posobj.transform.position = rematch.point; //ÀÓ½Ã    È®ÀÎ¿ë °ª. Å×½ºÆ® Á¾·á·Î ÀÎÇØ ÁÖ¼®Ã³¸®
                        chacker = hit;
                        //Debug($"Hit Position: {rematch.point}"); //ÀÓ½Ã È®ÀÎ¿ë °ª. Å×½ºÆ® Á¾·á·Î ÀÎÇØ ÁÖ¼®Ã³¸®
                        Debug.DrawRay(rematch.point, Vector2.up * 0.1f, Color.green, 100f); //½ºÆùÀ§Ä¡ Ã¼Å©¿ë.
                    }



                }
            }
        }


        public Vector2 PointSetting(Vector2 min, Vector2 max)
        {
            float xpos = Random.Range(min.x, max.x);
            float ypos = Random.Range(min.y, max.y);
            //Vector2 pos = new Vector2(xpos, ypos); 
            Debug.Log(xpos + ", " + ypos);

            return new Vector2(xpos, ypos);
        }

        public Vector2 GroundPos(Vector2 pos)
        {
            bool chacker = false;
            while (!chacker)
            {
                int layerMask = LayerMask.GetMask("Ground");
                RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.down, mapsizey, layerMask);

                if (hit)
                {
                    hit.point = new Vector2(hit.point.x, hit.point.y + tilesize);
                    RaycastHit2D rematch = Physics2D.Raycast(hit.point, Vector2.down, mapsizey, layerMask);

                    pos = rematch.point;
                    chacker = hit;
                    Debug.DrawRay(rematch.point, Vector2.up * 0.1f, Color.green, 100f);
                }
            }
            return pos;
        }

    }




}


