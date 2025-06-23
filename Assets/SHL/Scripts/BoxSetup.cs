using UnityEngine;
using System.Collections.Generic;
namespace SHL
{
    public class BoxSetup : MonoBehaviour
    {
       
       // public GameObject[] spawnPoints; // 박스를 활성화할    위치들
        public List<GameObject> spawnPoints; //코드변경
        public int boxCount = 4; // 활성화할 박스의 개수
        private void Start()
        {
            SpawnBoxes();
        }
        void SpawnBoxes()
        {
            
            for (int i = 0; i < boxCount;i++)
            {
                // 랜덤한 위치에서 박스 생성
                int randomIndex = Random.Range(0, spawnPoints.Count);
                
                
                    spawnPoints[randomIndex].SetActive(true);
                    Debug.Log("Box spawned at: " + spawnPoints[randomIndex].name);
                    
                    spawnPoints.RemoveAt(randomIndex); // 생성된 박스를 목록에서 제거
                
                

            }
        }

    }
}