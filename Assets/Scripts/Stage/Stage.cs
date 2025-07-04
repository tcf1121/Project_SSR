using UnityEngine;

public class Stage : MonoBehaviour
{
    public int StageNum { get => _stageNum; }
    [SerializeField] private int _stageNum;
    public GameObject SkyObj { get => _skyObj; }
    [SerializeField] private GameObject _skyObj;
}

