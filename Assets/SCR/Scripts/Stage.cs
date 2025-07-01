using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCR
{
    public class Stage : MonoBehaviour
    {
        public int StageNum { get => _stageNum; }
        [SerializeField] private int _stageNum;
        public GameObject SkyObj { get => _skyObj; }
        [SerializeField] private GameObject _skyObj;
    }
}

