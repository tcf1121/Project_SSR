using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utill;

namespace SCR
{
    public class BoxSpawner : MonoBehaviour
    {
        [SerializeField] private StageManager stageManager;
        [SerializeField] private SpriteRenderer mapSize;
        private List<Vector2> _spwanPoint;
        private Vector2 min;
        private Vector2 max;

        private void Awake() => Init();

        private void Init()
        {

        }

        void Start()
        {
            mapSize = stageManager.StageMap.GetComponent<SpriteRenderer>();
            SetPos();
            Spawn();
        }

        private void SetPos()
        {
            _spwanPoint = new();
            min = mapSize.bounds.min;
            max = mapSize.bounds.max;
            _spwanPoint = RandomPosCreater.RandomPosList(min, max, ObjectPool.GetPool(EPoolObjectType.Box).Pool.Count);
        }

        private void Spawn()
        {
            for (int i = 0; i < ObjectPool.GetPool(EPoolObjectType.Box).PoolCount; i++)
            {
                GameObject box = ObjectPool.TakeFromPool(EPoolObjectType.Box);
                box.transform.position = _spwanPoint[0];
                _spwanPoint.RemoveAt(0);
            }

        }
    }


}

