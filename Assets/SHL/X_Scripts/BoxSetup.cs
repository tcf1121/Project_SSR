using UnityEngine;
using System.Collections.Generic;
using Utill;

namespace SHL

{
    public class BoxSetup : MonoBehaviour
    {
        public static BoxSetup instance;
        // public GameObject[] spawnPoints; // �ڽ��� Ȱ��ȭ��    ��ġ��
        public List<Vector2> _spawnPoints => spawnPoints;
        [SerializeField] private List<Vector2> spawnPoints; //�ڵ庯��
        [SerializeField] private List<Vector2> teleportPoint;  //�ڷ����� ��ġ�� �߰�  �и�.

        public int boxCount = 4; // Ȱ��ȭ�� �ڽ��� ����
        [SerializeField] private float mapsizex;
        [SerializeField] private float mapsizey;
        [SerializeField] private GameObject boxPrefab; // �ڽ� ������
        [SerializeField] private GameObject TeleporterPrefab; // �ڷ����� ������
        Vector2 min;
        Vector2 max;
        [Header("�����ɼ�")]
        [SerializeField] private bool showGizmos = true; // Inspector���� �Ѱ� ����
        [SerializeField] private Color gizmoColor = Color.green;
        [SerializeField] private Color teleGizmo = Color.red;
        [SerializeField] private float radius = 0.3f;
        private void Start()
        {
            instance = this;
            min = new Vector2(-mapsizex / 2, -mapsizey / 2);
            max = new Vector2(mapsizex / 2, mapsizey / 2);
            spawnPoints = BoxSpawn(boxCount); // �ڽ� ��ġ ����
            SpawnBoxes();
            //Debug.Log($"{min},{max}"); // ����׿� �α�
            Teleporter();
        }
        void SpawnBoxes()
        {

            for (int i = 0; i < boxCount; i++)
            {
                // ������ ��ġ���� �ڽ� ����
                Transform create =Instantiate(boxPrefab).transform;
                //GameObject create = Instantiate(boxPrefab);
                create.parent = transform; // �ڽ��� ���� ������Ʈ�� �ڽ����� ����
                create.position = spawnPoints[i]; // �ڽ� ��ġ ����

            }
        }
       List<Vector2> BoxSpawn(int count)
        {
            return RandomPosCreater.RandomPosList(min, max, count);

        }
        void Teleporter()
        {
            teleportPoint = BoxSpawn(boxCount); // ��ġ �缳��.
            // �ڷ����� ����
            Transform teleporter = Instantiate(TeleporterPrefab).transform; 
            teleporter.parent = transform; // �ڷ����͸� ���� ������Ʈ�� �ڽ����� ����
            teleporter.position = teleportPoint[0];//[Random.Range(0, boxCount)]; // ������ ��ġ�� �ڷ����� ����
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