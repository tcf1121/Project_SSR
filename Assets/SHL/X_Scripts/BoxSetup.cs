using UnityEngine;
using System.Collections.Generic;
using Utill;

namespace SHL

{
    public class BoxSetup : MonoBehaviour
    {
        public static BoxSetup instance;
        // public GameObject[] spawnPoints; // 박스를 활성화할    위치들
        public List<Vector2> _spawnPoints => spawnPoints;
        [SerializeField] private List<Vector2> spawnPoints; //코드변경
        [SerializeField] private List<Vector2> teleportPoint;  //텔레포터 위치값 추가  분리.

        public int boxCount = 4; // 활성화할 박스의 개수
        [SerializeField] private float mapsizex;
        [SerializeField] private float mapsizey;
        [SerializeField] private GameObject boxPrefab; // 박스 프리팹
        [SerializeField] private GameObject TeleporterPrefab; // 텔레포터 프리팹
        Vector2 min;
        Vector2 max;
        [Header("기즈모옵션")]
        [SerializeField] private bool showGizmos = true; // Inspector에서 켜고 끄기
        [SerializeField] private Color gizmoColor = Color.green;
        [SerializeField] private Color teleGizmo = Color.red;
        [SerializeField] private float radius = 0.3f;
        private void Start()
        {
            instance = this;
            min = new Vector2(-mapsizex / 2, -mapsizey / 2);
            max = new Vector2(mapsizex / 2, mapsizey / 2);
            spawnPoints = BoxSpawn(boxCount); // 박스 위치 생성
            SpawnBoxes();
            //Debug.Log($"{min},{max}"); // 디버그용 로그
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
       List<Vector2> BoxSpawn(int count)
        {
            return RandomPosCreater.RandomPosList(min, max, count);

        }
        void Teleporter()
        {
            teleportPoint = BoxSpawn(boxCount); // 위치 재설정.
            // 텔레포터 생성
            Transform teleporter = Instantiate(TeleporterPrefab).transform; 
            teleporter.parent = transform; // 텔레포터를 현재 오브젝트의 자식으로 설정
            teleporter.position = teleportPoint[0];//[Random.Range(0, boxCount)]; // 랜덤한 위치에 텔레포터 생성
        }
        private void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;
            Gizmos.color = gizmoColor;
            foreach (var pos in spawnPoints)
            {
                Gizmos.DrawSphere(pos, 0.3f);
            }

            Gizmos.color = teleGizmo;
            foreach (var pos in teleportPoint)
            {
                Gizmos.DrawWireSphere(pos, 0.3f);
            }
        }
    }
    
    }