using System.Collections.Generic;
using UnityEngine;
namespace Utill
{
    public static class RandomPosCreater
    {

        /// <summary>
        /// 특정한 공간 내에 랜덤한 좌표 생성
        /// </summary>
        /// <param name="min">랜덤 좌표를 생성할 공간의 좌하단 좌표</param>
        /// <param name="max">랜덤 좌표를 생성할 공간의 우상단 좌표</param>
        /// <param name="IsGround">땅 좌표를 생성할 것인지 아닌지 확인</param>
        /// <returns></returns>
        public static Vector2 RandomPos(Vector2 min, Vector2 max, bool IsGround = false)
        {
            if (IsGround) return RandomGroundPos(min, max);
            else return RandomAllPos(min, max);
        }


        /// <summary>
        /// 특정한 공간 내에 랜덤한 좌표 생성(외부에서 사용 x)
        /// </summary>
        /// <param name="min">랜덤 좌표를 생성할 공간의 좌하단 좌표</param>
        /// <param name="max">랜덤 좌표를 생성할 공간의 우상단 좌표</param>
        /// <returns></returns>
        private static Vector2 RandomAllPos(Vector2 min, Vector2 max)
        {
            float xpos = Random.Range(min.x, max.x);
            float ypos = Random.Range(min.y, max.y);

            return new Vector2(xpos, ypos);
        }


        /// <summary>
        /// 특정한 공간 내에 랜덤한 땅의 좌표 생성(외부에서 사용 x)
        /// </summary>
        /// <param name="min">랜덤 좌표를 생성할 공간의 좌하단 좌표</param>
        /// <param name="max">랜덤 좌표를 생성할 공간의 우상단 좌표</param>
        /// <param name="yLength">랜던 좌표를 생성할 공간의 Y축 길이</param>
        /// <returns></returns>
        private static Vector2 RandomGroundPos(Vector2 min, Vector2 max)
        {
            Vector2 groundPos;
            float yLength = max.y - min.y;
            bool hitGround = false;
            LayerMask _allGroundLayers = (1 << 9) | (1 << 10); // 모든 바닥
            int layerMask = _allGroundLayers;
            int time = 0;
            do
            {
                groundPos = RandomAllPos(min, max);
                RaycastHit2D hit = Physics2D.Raycast(groundPos, Vector2.down, yLength, layerMask);
                time++;
                if (hit.collider != null)
                {
                    groundPos.y = hit.point.y + 1;
                    RaycastHit2D hitdouble = Physics2D.Raycast(groundPos, Vector2.down, yLength, layerMask);
                    if (hitdouble.collider != null)
                        groundPos.y = hit.point.y;
                    hitGround = true;
                }
                if (time > 10)
                    break;
            } while (!hitGround);
            return groundPos;
        }

        /// <summary>
        /// 특정한 공간을 나누어 나눈 수만큼 랜덤한 땅 좌표들 생성
        /// </summary>
        /// <param name="min">랜덤 좌표를 생성할 공간의 좌하단 좌표</param>
        /// <param name="max">랜덤 좌표를 생성할 공간의 우상단 좌표</param>
        /// <param name="num">공간을 나눌 수</param>
        /// <returns></returns>
        public static List<Vector2> RandomPosList(Vector2 min, Vector2 max, int num)
        {
            List<Vector2> randomPosList = new();
            float divideX = (max.x - min.x) / num;

            for (int i = 0; i < num; i++)
                randomPosList.Add(RandomGroundPos(
                    new Vector2(min.x + (divideX * i), min.y),
                    new Vector2(min.x + (divideX * i + 1), max.y)
                    ));
            {
                return randomPosList;
            }
        }


    }




}


