using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utill
{
    public class Spwaner : MonoBehaviour
    {
        public List<Vector2> SpwanPoint { get { return _spwanPoint; } set { _spwanPoint = value; } }
        [SerializeField] private List<Vector2> _spwanPoint;
        public PoolInfo PoolInfo { get { return _poolInfo; } set { _poolInfo = value; } }
        [SerializeField] private PoolInfo _poolInfo;

        void Awake() => Init();

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
            _spwanPoint = new();
            for (int i = 0; i < _poolInfo.PoolCount; i++)
            {
                _spwanPoint.Add(new Vector2(0, i * 10));
            }
        }

        private void Spawn()
        {
            GameObject spwanGO = ObjectPool.TakeFromPool(EPoolObjectType.Object);
            int randIndex = Random.Range(0, _spwanPoint.Count);
            spwanGO.transform.position = _spwanPoint[randIndex];
            _spwanPoint.RemoveAt(randIndex);
        }
    }
}