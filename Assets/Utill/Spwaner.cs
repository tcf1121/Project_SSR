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
            if (_spwanPoint.Count == 0)
            {
                SetPos();
            }
            Spawn();
        }

        private void SetPos()
        {
            _spwanPoint = new();
            for (int i = 0; i < objectPool.PoolCount; i++)
            {

                GameObject spawobj = new();
                spawobj.transform.position = new Vector3(0, 0, 0);
                _spwanPoint.Add(spawobj.transform);
            }
        }

        private void Spawn()
        {
            foreach (GameObject spwanGO in objectPool.Pool)
            {
                int randIndex = Random.Range(0, _spwanPoint.Count);
                spwanGO.transform.position = _spwanPoint[randIndex].transform.position;
                Debug.Log(spwanGO.transform.position);
                _spwanPoint.RemoveAt(randIndex);
            }
        }
    }
}

