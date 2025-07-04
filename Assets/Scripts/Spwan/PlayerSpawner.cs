using System.Collections.Generic;
using UnityEngine;
using Utill;


public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private StageManager stageManager;
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
        int dir = Random.Range(2, 6);
        GameManager.Player.gameObject.transform.position = _spwanPoint[dir];
    }
}

