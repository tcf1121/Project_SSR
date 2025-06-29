using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utill;

namespace SCR
{
    public class MonsterSpwaner : MonoBehaviour
    {

        public ObjectPool ObjectPool { get { return _objectPool; } set { _objectPool = value; } }
        [SerializeField] private ObjectPool _objectPool;

        [Header("스폰 설정")]
        [SerializeField] private float spawnInterval = 5f; // 스폰 간격
        [SerializeField] private int maxAlive = 10;
        [SerializeField] private int Alive = 0;
        [SerializeField] private int _creadit = 3;
        private Vector2 min;
        private Vector2 max;

        [SerializeField] private List<Monster> monsters;
        private List<Vector2> _spwanPoint;
        private List<Monster> _respwanMonsters;

        void Awake() => Init();

        private void Init()
        {
            _objectPool = GetComponent<ObjectPool>();
            _respwanMonsters = new();
            _spwanPoint = new();
        }

        void Start()
        {
            StartCoroutine(SpawnRoutine());
        }





        private void SetMonster()
        {
            while (true)
            {
                List<Monster> FulfillConMon = monsters.FindAll(n => n.Credit <= _creadit);
                if (FulfillConMon.Count > 0)
                {
                    Monster setMonster = FulfillConMon[Random.Range(0, FulfillConMon.Count)];
                    _creadit -= setMonster.Credit;
                    _respwanMonsters.Add(setMonster);
                }
                else
                    break;
            }
        }

        private void SetPos()
        {
            min = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
            max = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));
            for (int i = 0; i < _respwanMonsters.Count; i++)
            {
                _spwanPoint.Add(RandomPosCreater.RandomPos(min, max, _respwanMonsters[i].IsGround));
            }

        }

        private bool CanSpwan()
        {
            return maxAlive - Alive > 0 ? true : false;
        }

        private void Spawn()
        {
            SetMonster();
            if (_respwanMonsters.Count > 0)
            {
                SetPos();
                foreach (GameObject mob in _objectPool.Pool)
                {
                    if (!mob.activeSelf)
                    {
                        if (_respwanMonsters.Count == 0)
                            break;
                        mob.GetComponent<Monster>().SetMonster(_respwanMonsters[0]);
                        mob.transform.position = _spwanPoint[0];
                        _objectPool.TakeFromPool(mob);
                        _respwanMonsters.RemoveAt(0);
                        _spwanPoint.RemoveAt(0);
                    }
                }
            }
            _respwanMonsters.Clear();
            _spwanPoint.Clear();
        }


        private IEnumerator SpawnRoutine()
        {
            float currentSpawnInterval;
            while (true)
            {
                currentSpawnInterval = spawnInterval;
                if (CanSpwan())
                    Spawn();
                while (currentSpawnInterval > 0.0f)
                {
                    currentSpawnInterval -= Time.deltaTime;
                    yield return new WaitForFixedUpdate();
                }
            }
        }
    }
}