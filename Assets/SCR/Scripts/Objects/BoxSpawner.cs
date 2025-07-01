using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utill;

namespace SCR
{
    public class BoxSpawner : MonoBehaviour
    {
        [SerializeField] private StageManager stageManager;
        public ObjectPool ObjectPool { get { return _objectPool; } set { _objectPool = value; } }
        [SerializeField] private ObjectPool _objectPool;
        [SerializeField] private SpriteRenderer mapSize;
        private List<Vector2> _spwanPoint;
        private Vector2 min;
        private Vector2 max;

        private void Awake() => Init();

        private void Init()
        {
            _objectPool = GetComponent<ObjectPool>();
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
            _spwanPoint = RandomPosCreater.RandomPosList(min, max, _objectPool.Pool.Count);
        }

        private void Spawn()
        {

            foreach (GameObject box in _objectPool.Pool)
            {
                box.transform.position = _spwanPoint[0];
                _spwanPoint.RemoveAt(0);
                box.SetActive(true);
            }
        }
    }


}

