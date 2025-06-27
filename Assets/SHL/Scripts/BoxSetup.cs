using UnityEngine;
using System.Collections.Generic;
using Utill;

namespace SHL

{
    public class BoxSetup : MonoBehaviour
    {

        // public GameObject[] spawnPoints; // �ڽ��� Ȱ��ȭ��    ��ġ��
        public List<Vector2> spawnPoints; //�ڵ庯��
        public int boxCount = 4; // Ȱ��ȭ�� �ڽ��� ����
        [SerializeField] private float mapsizex;
        [SerializeField] private float mapsizey;
        [SerializeField] private GameObject boxPrefab; // �ڽ� ������
        [SerializeField] private GameObject TeleporterPrefab; // �ڷ����� ������
        Vector2 min;
        Vector2 max;
        private void Start()
        {
            min = new Vector2(-mapsizex / 2, -mapsizey / 2);
            max = new Vector2(mapsizex / 2, mapsizey / 2);
            BoxSpawn(); // �ڽ� ��ġ ����
            SpawnBoxes();
            Debug.Log($"{min},{max}"); // ����׿� �α�
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
        void BoxSpawn()
        {
            spawnPoints = RandomPosCreater.RandomPosList(min, max, boxCount);

        }
        void Teleporter()
        {
            BoxSpawn(); // ��ġ �缳��.
            // �ڷ����� ����
            Transform teleporter = Instantiate(TeleporterPrefab).transform; 
            teleporter.parent = transform; // �ڷ����͸� ���� ������Ʈ�� �ڽ����� ����
            teleporter.position = spawnPoints[Random.Range(0, boxCount)]; // ������ ��ġ�� �ڷ����� ����
        }
    }
}