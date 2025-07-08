using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> bossPrefab; // 보스 프리팹

    public GameObject StageBoss()
    {
        if (GameManager.Stage == 3)
            return null;
        else if (GameManager.Stage == 4)
            return bossPrefab[bossPrefab.Count - 1];
        else
        {
            return bossPrefab[GameManager.Stage - 1];
        }
    }
}
