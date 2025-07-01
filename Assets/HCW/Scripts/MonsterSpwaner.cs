using System.Collections;
using System.Collections.Generic;
using PHG;
using UnityEngine;
using Utill;

namespace HCW
{
    public class MonsterSpwaner : MonoBehaviour
    {
        public List<Vector2> SpwanPoint { get { return _spwanPoint; } set { _spwanPoint = value; } }
        [SerializeField] private List<Vector2> _spwanPoint;
        public PoolInfo PoolInfo { get { return _poolInfo; } set { _poolInfo = value; } }
        [SerializeField] private PoolInfo _poolInfo;

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
            for (int i = 0; i < _poolInfo.PoolCount; i++)
            {
                _spwanPoint.Add(RandomPosCreater.RandomPos(min, max, true));
            }
        }

        private void Spawn()
        {
            int aliveCount = 0; // 현재 활성화된 몬스터 수
            foreach (GameObject mob in _poolInfo.Pool)
                if (mob.activeSelf)
                    aliveCount++;

            int spawnCount = _poolInfo.PoolCount - aliveCount; // 최대 활성화 가능한 몬스터 수
            if (spawnCount <= 0) return;

            List<Vector2> spawnPoints = new List<Vector2>(_spwanPoint);

            foreach (GameObject mob in _poolInfo.Pool) // 몬스터 풀에서 활성화된 몬스터를 확인
            {
                if (!mob.activeSelf && spawnCount > 0)
                {
                    bool isFlying = mob.GetComponent<FlyingTag>() != null; // FlyingTag 컴포넌트 확인
                    Vector2 pos = isFlying
                        ? RandomPosCreater.RandomPos(min, max, false)
                        : RandomPosCreater.RandomPos(min, max, true);

                    mob.transform.position = pos;
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