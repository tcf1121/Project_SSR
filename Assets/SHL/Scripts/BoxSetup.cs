using UnityEngine;
using System.Collections.Generic;
using Utill;

namespace SHL

{
    public class BoxSetup : MonoBehaviour
    {

        // public GameObject[] spawnPoints; // 박스를 활성화할    위치들
        public List<Vector2> spawnPoints; //코드변경
        public int boxCount = 4; // 활성화할 박스의 개수
        [SerializeField] private float mapsizex;
        [SerializeField] private float mapsizey;
        [SerializeField] private GameObject boxPrefab; // 박스 프리팹
        [SerializeField] private GameObject TeleporterPrefab; // 텔레포터 프리팹
        Vector2 min;
        Vector2 max;
        private void Start()
        {
            min = new Vector2(-mapsizex / 2, -mapsizey / 2);
            max = new Vector2(mapsizex / 2, mapsizey / 2);
            BoxSpawn(); // 박스 위치 생성
            SpawnBoxes();
            Debug.Log($"{min},{max}"); // 디버그용 로그
            Teleporter();
        }
        void SpawnBoxes()
        {

            for (int i = 0; i < boxCount; i++)
            {
                // 랜덤한 위치에서 박스 생성
                Transform create =Instantiate(boxPrefab).transform;
                //GameObject create = Instantiate(boxPrefab);
                create.parent = transform; // 박스를 현재 오브젝트의 자식으로 설정
                create.position = spawnPoints[i]; // 박스 위치 설정

            }
        }
        void BoxSpawn()
        {
            spawnPoints = RandomPosCreater.RandomPosList(min, max, boxCount);

        }
        void Teleporter()
        {
            BoxSpawn(); // 위치 재설정.
            // 텔레포터 생성
            Transform teleporter = Instantiate(TeleporterPrefab).transform; 
            teleporter.parent = transform; // 텔레포터를 현재 오브젝트의 자식으로 설정
            teleporter.position = spawnPoints[Random.Range(0, boxCount)]; // 랜덤한 위치에 텔레포터 생성
        }
    }
}