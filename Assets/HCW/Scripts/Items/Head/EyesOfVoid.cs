using System.Collections;
using LHE;
using SCR;
using UnityEngine;

namespace HCW
{
    public class EyesOfVoid : Item
    {
        public GameObject voidEyeLaserPrefab;
        public Transform laserPoint;

        [SerializeField] private float baseDamage = 1f; 
        [SerializeField] private float bossDamageMultiplier = 2.0f;
        [SerializeField] private float tickInterval = 0.5f; // 틱 간격
        [SerializeField] private float laserDuration = 3f;

        private int stackCount = 1; // 아이템 스택 수
        [SerializeField] private float damageStack = 0.2f;

        private Transform playerTransform;
        private LHE.PlayerStats playerStats;

        override protected void Init()
        {
            base.Init();
            playerTransform = GameObject.FindWithTag("Player").transform;
            playerStats = playerTransform.GetComponent<LHE.PlayerStats>();
        }


    }
}