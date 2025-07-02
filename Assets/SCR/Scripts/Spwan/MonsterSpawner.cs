using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utill;

namespace SCR
{
    public class MonsterSpawner : MonoBehaviour
    {

        [Header("스폰 설정")]
        [SerializeField] private float minSpawnInterval = 7f; // 최소 스폰 간격
        [SerializeField] private float maxSpawnInterval = 13f; // 최대 스폰 간격
        [SerializeField] private int maxAlive = 10;
        [SerializeField] private int Alive = 0;
        public float Credit { get => _creadit; }
        [SerializeField] private float _creadit = 3;

        private Vector2 min;
        private Vector2 max;

        [SerializeField] private List<Monster> monsters;
        private List<Vector2> _spwanPoint;
        private List<Monster> _respwanMonsters;

        private Coroutine _spawnCor;
        private Coroutine _creditCor;

        void Awake() => Init();

        private void Init()
        {
            _respwanMonsters = new();
            _spwanPoint = new();
        }

        void Start()
        {
            _spawnCor = StartCoroutine(SpawnRoutine());
            _creditCor = StartCoroutine(GetCredit());
        }





        private void SetMonster()
        {
            while (true)
            {
                List<Monster> FulfillConMon = monsters.FindAll(n => n.Credit <= _creadit /*&&n.Credit >= 최소 크레딧 */);
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
                GameObject monster;
                for (int i = 0; i < _respwanMonsters.Count; i++)
                {
                    monster = ObjectPool.TakeFromPool(EPoolObjectType.Monster);
                    monster.GetComponent<Monster>().Clone(_respwanMonsters[0]);
                    monster.transform.position = _spwanPoint[0];
                    _respwanMonsters.RemoveAt(0);
                    _spwanPoint.RemoveAt(0);

                }
            }
            _respwanMonsters.Clear();
            _spwanPoint.Clear();
        }

        private IEnumerator GetCredit()
        {
            float currentTime;
            while (true)
            {
                currentTime = 1f;
                while (currentTime > 0.0f)
                {
                    currentTime -= Time.deltaTime;
                    yield return new WaitForFixedUpdate();
                }
                _creadit += 0.21f * (1f + 0.4f * GameManager.StageManager.DangerIndexManager.GetDangerIndex());
            }
        }


        private IEnumerator SpawnRoutine()
        {
            float currentSpawnInterval;
            while (true)
            {
                currentSpawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
                while (currentSpawnInterval > 0.0f)
                {
                    currentSpawnInterval -= Time.deltaTime;
                    yield return new WaitForFixedUpdate();
                }
                if (CanSpwan())
                    Spawn();
            }
        }
    }
}