using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Utill
{
    public class ObjectPool : MonoBehaviour
    {
        public static ObjectPool instance;

        public int PoolCount;
        public List<GameObject> overlappingPrefab;

        public Queue<GameObject> Pool;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(this.gameObject);
            Init();
        }

        private void Init()
        {
            Pool = new();

            // 미리 오브젝트 생성 해놓기
            for (int i = 0; i < PoolCount; i++)
            {
                CreatePoolObject(i);
            }
        }

        // 생성
        private void CreatePoolObject(int num)
        {
            GameObject poolGO = Instantiate(overlappingPrefab[num % overlappingPrefab.Count]);
            poolGO.SetActive(false);
            poolGO.transform.parent = transform;
            Pool.Enqueue(poolGO);
        }

        // 사용
        public void TakeFromPool(GameObject poolGo)
        {
            poolGo.SetActive(true);
        }

        // 반환
        public void ReturnPool(GameObject poolGo)
        {
            poolGo.SetActive(false);
        }

    }
}

