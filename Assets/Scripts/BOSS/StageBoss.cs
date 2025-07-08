using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageBoss : MonoBehaviour
{
    [SerializeField] private List<GameObject> BossPrefab;

    public GameObject GetStageBoss()
    {
        if (GameManager.Stage == 4)
            return BossPrefab[BossPrefab.Count - 1];
        else if (GameManager.Stage == 3)
            return null;
        else
            return BossPrefab[GameManager.Stage - 1];
    }
}
