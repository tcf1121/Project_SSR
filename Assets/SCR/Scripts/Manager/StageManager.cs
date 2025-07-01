using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace SCR
{
    public class StageManager : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _mapLists;
        [SerializeField] CinemachineVirtualCamera _camera;
        [SerializeField] CinemachineConfiner _confiner;
        public ItemSpawner ItemSpawner { get => _itemSpawner; }
        [SerializeField] private ItemSpawner _itemSpawner;
        public GameObject StageMap { get => stageMap; }
        [SerializeField] private GameObject stageMap;


        void Awake()
        {
            RandomMap();
            SetPlayer();
            GameManager.SetStageManager(this);
        }

        private void SetPlayer()
        {
            _camera.Follow = GameManager.Player.transform;
        }

        private void RandomMap()
        {
            List<GameObject> stageList = _mapLists.FindAll(n => n.GetComponent<Stage>().StageNum == GameManager.Stage);
            GameObject stage = Instantiate(stageList[Random.Range(0, stageList.Count)]);
            stageMap = stage.GetComponent<Stage>().SkyObj;
            _confiner.m_BoundingShape2D = stageMap.GetComponent<Collider2D>();
        }
    }
}

