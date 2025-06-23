using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utill
{
    public class Spwaner : MonoBehaviour
    {
        [SerializeField] private List<Transform> _spwanPoint;
        [SerializeField] private ObjectPool objectPool;

        void Awake() => Init();

        private void Init()
        {
            Spawn();
        }

        private void Spawn()
        {
            foreach (GameObject spwanGO in objectPool.Pool)
            {
                int randIndex = Random.Range(0, _spwanPoint.Count);
                spwanGO.transform.position = _spwanPoint[randIndex].position;
                _spwanPoint.RemoveAt(randIndex);
            }
        }
    }
}

