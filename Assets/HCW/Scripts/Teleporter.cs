using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HCW
{
    public class Teleporter : MonoBehaviour
    {
        [Header("상호작용")]
        [SerializeField] private KeyCode interactionKey = KeyCode.F; // 상호작용 키

        [Header("이벤트")]
        [SerializeField] private GameObject bossPrefab; // 보스 프리팹
        [SerializeField] Transform bossSpawnPoint; // 보스 스폰 위치
        [SerializeField] private float eventDuration = 90f; // 이벤트 지속 시간
        // [SerializeField] 몬스터 프리팹들 (몬스터 스폰은 텔레포터에 없어도 될듯?)
        // [SerializeField] 몬스터 스폰 위치들

        [Header("씬 전환")]
        [SerializeField] private string nextSceneName; // 다음 씬 이름

        private bool isPlayerTouch = false; // 플레이어가 닿은 상태 인지
        private bool isEventStart = false; // 이벤트 시작 여부
        private bool isTimerFinished = false; // 90초 타이머가 끝났는지
        private bool isBossDied = false; // 보스 사망여부
        private bool isTeleporterActivated = false; // 텔레포터가 다음 스테이지로 넘어갈 준비가 되었는지

        private GameObject currentBoss; // 소환된 보스

        private void Awake()
        {
            if (bossSpawnPoint == null)
            {
                bossSpawnPoint = this.transform;
            }

        }
        private void Update()
        {
            if (isPlayerTouch && Input.GetKeyDown(interactionKey))
            {
                // 이벤트가 시작되지 않았으면 이벤트 시작
                if (!isEventStart)
                {
                    StartTeleporterEvent();
                }
                
                else if (isTeleporterActivated)
                {
                    LoadNextScene();
                }
            }

            if (isEventStart && !isTeleporterActivated) // 
            {
                CheckEventCompletion();
            }
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player")) // Player 태그를 가진 오브젝트와 충돌 시
            {
                isPlayerTouch = true;
            }
        }
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerTouch = false;
            }
        }
        private void StartTeleporterEvent()
        {
            if (isEventStart) return; // 중복 확인

            isEventStart = true;

            SpawnBoss();

            // 몬스터 스포너 가속?

            StartCoroutine(EventTimerRoutine());

        }
        private void CheckEventCompletion()
        {
            // 보스가 죽었는지 확인 (보스 오브젝트가 파괴되면 null이 됨)
            if (!isBossDied && currentBoss == null)
            {
                isBossDied = true;
            }

            // 타이머와 보스 처치가 모두 완료되면 텔레포터 활성화
            if (isTimerFinished && isBossDied)
            {
                ActivateTeleporter();
            }
        }

        private void ActivateTeleporter()
        {
            isTeleporterActivated = true;

            // 몬스터 스포너 중지
        }

        private void SpawnBoss()
        {
            if (bossPrefab != null)
            {
                // 보스 스폰위치가 설정되어 있으면 해당 위치에 소환 아니면 텔레포터 위치에 소환
                Vector3 spawnPos = (bossSpawnPoint != null) ? bossSpawnPoint.position : transform.position;
                currentBoss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
            }
        }

        private IEnumerator EventTimerRoutine()
        {
            // 90초 타이머 시작
            yield return new WaitForSeconds(eventDuration);
            isTimerFinished = true;
        }

        private void LoadNextScene()
        {
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }

    }
}
