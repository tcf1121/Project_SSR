using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utill;

public class MonsterSpawner : MonoBehaviour
{

    [Header("스폰 설정")]
    [SerializeField] private float minSpawnInterval = 7f; // 최소 스폰 간격
    [SerializeField] private float maxSpawnInterval = 13f; // 최대 스폰 간격
    [SerializeField] private int maxAlive = 10;
    [SerializeField] private int Alive = 0;
    public float Credit { get => _creadit; }
    [SerializeField] private float _creadit = 0;
    private int minCreadit;

    private Vector2 min;
    private Vector2 max;

    [SerializeField] private List<Monster> monsters;
    private List<Monster>[] _typeMonsters;

    private Vector2 _spwanPoint;
    private Monster _respwanMonsters;
    private MonsterType _monsterType;
    private bool _canSpwan;

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
        ClassificationMonster();
        _spawnCor = StartCoroutine(SpawnRoutine());
        _creditCor = StartCoroutine(GetCredit());
    }

    private void ClassificationMonster()
    {
        _typeMonsters = new List<Monster>[3];
        for (int i = 0; i < 3; i++)
            _typeMonsters[i] = new();
        foreach (Monster monster in monsters)
            _typeMonsters[(int)monster.MonsterType].Add(monster);
    }

    private void SetMinCreadit()
    {
        minCreadit = (int)GameManager.StageManager.DangerIndexManager.GetDangerIndex();
        if (minCreadit > 2) minCreadit = 2;
    }

    private void SetMonterType()
    {
        _monsterType = (MonsterType)Random.Range(0, 3);
    }

    private EPoolObjectType GetMonsterType()
    {
        if (_monsterType == MonsterType.CDMonster)
            return EPoolObjectType.CDMonster;
        else if (_monsterType == MonsterType.LDMonster)
            return EPoolObjectType.LDMonster;
        else
            return EPoolObjectType.FlyMonster;
    }

    private void SetMonster()
    {
        SetMinCreadit();
        List<Monster> FulfillConMon = _typeMonsters[(int)_monsterType].FindAll(n => n.Credit <= _creadit && n.Credit >= minCreadit);
        if (FulfillConMon.Count > 0)
        {
            _respwanMonsters = FulfillConMon[Random.Range(0, FulfillConMon.Count)];
            _creadit -= _respwanMonsters.Credit;
        }
        else
        {
            _canSpwan = false;
        }
    }

    private void SetPos()
    {
        min = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        max = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));
        bool IsGround = _respwanMonsters.MonsterType != MonsterType.FlyMonster ? true : false;
        _spwanPoint = RandomPosCreater.RandomPos(min, max, IsGround);

    }

    private bool IsFull()
    {
        return maxAlive - Alive > 0 ? true : false;
    }

    public void DieMonster()
    {
        Alive--;
    }

    private void Spawn()
    {
        while (true)
        {
            SetMonterType();
            SetMonster();
            if (!_canSpwan) break;
            SetPos();
            GameObject monster = ObjectPool.TakeFromPool(GetMonsterType());
            monster.GetComponent<Monster>().Clone(_respwanMonsters);
            monster.transform.position = _spwanPoint;
        }

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
            _canSpwan = true;
            currentSpawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
            while (currentSpawnInterval > 0.0f)
            {
                currentSpawnInterval -= Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }
            if (IsFull())
                Spawn();
        }
    }
}