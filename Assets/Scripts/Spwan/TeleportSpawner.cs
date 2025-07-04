using System.Collections.Generic;
using UnityEngine;
using Utill;

public class TeleportSpawner : MonoBehaviour
{
    [SerializeField] private StageManager stageManager;
    [SerializeField] private GameObject TeleportPrefab;
    [SerializeField] private SpriteRenderer mapSize;
    private List<Vector2> _spwanPoint;
    private Vector2 min;
    private Vector2 max;

    void Start()
    {
        mapSize = stageManager.StageMap.GetComponent<SpriteRenderer>();
        SetPos();
        Spawn();
    }

    private void SetPos()
    {
        _spwanPoint = new();
        min = mapSize.bounds.min;
        max = mapSize.bounds.max;
        _spwanPoint = RandomPosCreater.RandomPosList(min, max, 8);
    }

    private void Spawn()
    {
        GameObject Teleport = Instantiate(TeleportPrefab);
        int dir = Random.Range(0, 2);
        if (dir == 0)
            Teleport.transform.position = _spwanPoint[0];
        else
            Teleport.transform.position = _spwanPoint[_spwanPoint.Count - 1];
    }
}