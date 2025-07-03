using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Utill
{
    public enum EPoolObjectType
    {
        Object,
        AttackItem,
        StatItem,
        CDMonster,
        LDMonster,
        FlyMonster,
        EquipAItem,
        EquipSItem
    }

    // 오브젝트 풀
    [Serializable]
    public class PoolInfo
    {
        public EPoolObjectType type;
        public int PoolCount;
        public GameObject overlappingPrefab;
        public GameObject container;

        public Queue<GameObject> Pool = new Queue<GameObject>();
    }
    public class ObjectPool : MonoBehaviour
    {
        public static ObjectPool instance;

        [SerializeField] private List<PoolInfo> poolInfoList;


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
            // 미리 오브젝트 생성 해놓기
            foreach (PoolInfo poolInfo in poolInfoList)
            {
                for (int i = 0; i < poolInfo.PoolCount; i++)
                {
                    CreatePoolObject(poolInfo);
                }
            }

        }

        // 생성
        private void CreatePoolObject(PoolInfo poolInfo)
        {
            GameObject poolGO = Instantiate(poolInfo.overlappingPrefab);
            poolGO.transform.parent = poolInfo.container.transform;
            poolGO.SetActive(false);
            poolInfo.Pool.Enqueue(poolGO);
        }

        // 생성
        private void CreatePoolObject(PoolInfo poolInfo, GameObject Prefab)
        {
            GameObject poolGO = Instantiate(Prefab);
            poolGO.transform.parent = poolInfo.container.transform;
            poolGO.SetActive(false);
            poolInfo.Pool.Enqueue(poolGO);
        }

        // ObjectType(Enum)으로 해당하는 PoolInfo를 반환해주는 함수
        public PoolInfo GetPoolByType(EPoolObjectType type)
        {
            foreach (PoolInfo poolInfo in poolInfoList)
            {
                if (type == poolInfo.type)
                {
                    return poolInfo;
                }
            }
            return null;
        }

        public static PoolInfo GetPool(EPoolObjectType type)
        {
            PoolInfo poolInfo = instance.GetPoolByType(type);
            return poolInfo;
        }

        // 사용
        public static GameObject TakeFromPool(EPoolObjectType type)
        {
            PoolInfo poolInfo = instance.GetPoolByType(type);
            GameObject objInstance = null;
            if (poolInfo.Pool.Count > 0)
            {
                objInstance = poolInfo.Pool.Dequeue();
            }
            else
            {
                instance.CreatePoolObject(poolInfo);
                objInstance = poolInfo.Pool.Dequeue();
            }
            objInstance.SetActive(true);
            return objInstance;
        }

        public static void PushPool(EPoolObjectType type, GameObject Prefab)
        {
            PoolInfo poolInfo = instance.GetPoolByType(type);
            instance.CreatePoolObject(poolInfo, Prefab);
        }

        // 반환
        public static void ReturnPool(GameObject poolGo, EPoolObjectType type)
        {
            PoolInfo poolInfo = instance.GetPoolByType(type);
            poolInfo.Pool.Enqueue(poolGo);
            poolGo.SetActive(false);
        }

    }
}

