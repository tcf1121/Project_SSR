using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utill
{
    public class Spwaner : MonoBehaviour
    {
        public List<Vector2> SpwanPoint { get { return _spwanPoint; } set { _spwanPoint = value; } }
        [SerializeField] private List<Vector2> _spwanPoint;
        public ObjectPool ObjectPool { get { return _objectPool; } set { _objectPool = value; } }
        [SerializeField] private ObjectPool _objectPool;

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
            for (int i = 0; i < _objectPool.PoolCount; i++)
            {
                _spwanPoint.Add(new Vector2(0, i * 10));
            }
        }

        private void Spawn()
        {
            foreach (GameObject spwanGO in _objectPool.Pool)
            {
                int randIndex = Random.Range(0, _spwanPoint.Count);
                spwanGO.transform.position = _spwanPoint[randIndex];
                Debug.Log(spwanGO.transform.position);
                _spwanPoint.RemoveAt(randIndex);
            }
        }
    }
}

