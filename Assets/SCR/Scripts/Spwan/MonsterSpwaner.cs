using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utill;

namespace SCR
{
    public class MonsterSpwaner : MonoBehaviour
    {
        public List<Vector2> SpwanPoint { get { return _spwanPoint; } set { _spwanPoint = value; } }
        [SerializeField] private List<Vector2> _spwanPoint;
        public ObjectPool ObjectPool { get { return _objectPool; } set { _objectPool = value; } }
        [SerializeField] private ObjectPool _objectPool;

        [Header("스폰 설정")]
        [SerializeField] private float spawnInterval = 5f; // 스폰 간격
        [SerializeField] private int maxAlive = 40;
        private Vector2 min;
        private Vector2 max;

        void Awake()
        {
            _spwanPoint = new();
            Init();
            StartCoroutine(SpawnRoutine());
        }
        private void Init()
        {
            if (_spwanPoint.Count == 0)
            {
                SetPos();
            }
            Spawn();
        }

        private void SetPos()
        {
            min = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
            max = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));
            _spwanPoint = new();
            for (int i = 0; i < _objectPool.PoolCount; i++)
            {
                _spwanPoint.Add(RandomPosCreater.RandomPos(min, max, true));
            }
        }

        private void Spawn()
        {
            int aliveCount = 0;
            Debug.Log(_objectPool.Pool);
            foreach (GameObject mob in _objectPool.Pool)
                if (mob.activeSelf)
                    aliveCount++;

            int spawnCount = _objectPool.PoolCount - aliveCount;
            if (spawnCount <= 0) return; // 최대치면 소환 안 함

            List<Vector2> spawnPoints = new List<Vector2>(_spwanPoint);

            foreach (GameObject mob in _objectPool.Pool)
            {
                if (!mob.activeSelf && spawnCount > 0)
                {
                    int randIndex = Random.Range(0, spawnPoints.Count);
                    mob.transform.position = spawnPoints[randIndex];
                    spawnPoints.RemoveAt(randIndex);

                    mob.SetActive(true);
                    spawnCount--;
                }
            }
        }
        private IEnumerator SpawnRoutine()
        {
            while (true)
            {
                Spawn();
                yield return new WaitForSeconds(spawnInterval);
            }
        }
    }
}