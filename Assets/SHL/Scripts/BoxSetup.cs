using UnityEngine;
using System.Collections.Generic;
namespace SHL
{
    public class BoxSetup : MonoBehaviour
    {
       
       // public GameObject[] spawnPoints; // �ڽ��� Ȱ��ȭ��    ��ġ��
        public List<GameObject> spawnPoints; //�ڵ庯��
        public int boxCount = 4; // Ȱ��ȭ�� �ڽ��� ����
        private void Start()
        {
            SpawnBoxes();
        }
        void SpawnBoxes()
        {
            
            for (int i = 0; i < boxCount;i++)
            {
                // ������ ��ġ���� �ڽ� ����
                int randomIndex = Random.Range(0, spawnPoints.Count);
                
                
                    spawnPoints[randomIndex].SetActive(true);
                    Debug.Log("Box spawned at: " + spawnPoints[randomIndex].name);
                    
                    spawnPoints.RemoveAt(randomIndex); // ������ �ڽ��� ��Ͽ��� ����
                
                

            }
        }

    }
}