
using System.Collections.Generic;
using UnityEngine;
using Utill;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private StageManager stageManager;
    [SerializeField] private SpriteRenderer mapSize;
    [SerializeField] private int _objectNum;
    [SerializeField] private List<GameObject> _objectPrefabs;
    private List<Vector2> _spwanPoint;
    private Vector2 min;
    private Vector2 max;

    private void Awake() => Init();

    private void Init()
    {
        _objectNum = 12;
    }

    void Start()
    {
        mapSize = stageManager.StageMap.GetComponent<SpriteRenderer>();
        SetPos();
        RandomObject();
        Spawn();
    }

    private void SetPos()
    {
        _spwanPoint = new();
        min = mapSize.bounds.min;
        max = mapSize.bounds.max;
        _spwanPoint = RandomPosCreater.RandomPosList(min, max, _objectNum);
    }

    private void RandomObject()
    {
        for (int i = 0; i < _objectNum - 2; i++)
        {
            int random = Random.Range(0, 100);
            if (random < 20)
                ObjectPool.PushPool(EPoolObjectType.Object, _objectPrefabs[0]);
            else if (random < 35)
                ObjectPool.PushPool(EPoolObjectType.Object, _objectPrefabs[1]);
            else if (random < 55)
                ObjectPool.PushPool(EPoolObjectType.Object, _objectPrefabs[2]);
            else if (random < 75)
                ObjectPool.PushPool(EPoolObjectType.Object, _objectPrefabs[3]);
            else if (random < 85)
                ObjectPool.PushPool(EPoolObjectType.Object, _objectPrefabs[4]);
            else
                ObjectPool.PushPool(EPoolObjectType.Object, _objectPrefabs[5]);
        }

    }

    private void Spawn()
    {
        for (int i = 0; i < _objectNum; i++)
        {
            GameObject box = ObjectPool.TakeFromPool(EPoolObjectType.Object);
            int random = Random.Range(0, _spwanPoint.Count);
            box.transform.position = _spwanPoint[random];
            _spwanPoint.RemoveAt(random);
        }

    }
}

