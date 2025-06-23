using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HCW
{
    public class Teleporter : MonoBehaviour
    {
        [Header("��ȣ�ۿ�")]
        [SerializeField] private KeyCode interactionKey = KeyCode.F; // ��ȣ�ۿ� Ű

        [Header("�̺�Ʈ")]
        [SerializeField] private GameObject bossPrefab; // ���� ������
        [SerializeField] Transform bossSpawnPoint; // ���� ���� ��ġ
        [SerializeField] private float eventDuration = 90f; // �̺�Ʈ ���� �ð�
        // [SerializeField] ���� �����յ� (���� ������ �ڷ����Ϳ� ��� �ɵ�?)
        // [SerializeField] ���� ���� ��ġ��

        [Header("�� ��ȯ")]
        [SerializeField] private string nextSceneName; // ���� �� �̸�

        private bool isPlayerTouch = false; // �÷��̾ ���� ���� ����
        private bool isEventStart = false; // �̺�Ʈ ���� ����
        private bool isTimerFinished = false; // 90�� Ÿ�̸Ӱ� ��������
        private bool isBossDied = false; // ���� �������
        private bool isTeleporterActivated = false; // �ڷ����Ͱ� ���� ���������� �Ѿ �غ� �Ǿ�����

        private GameObject currentBoss; // ��ȯ�� ����

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
                // �̺�Ʈ�� ���۵��� �ʾ����� �̺�Ʈ ����
                if (!isEventStart)
                {
                    StartTeleporterEvent();
                }
                
                else if (isTeleporterActivated)
                {
                    LoadNextScene();
                }
            }

            if (isEventStart && !isTeleporterActivated)
            {
                CheckEventCompletion();
            }
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player")) // Player �±׸� ���� ������Ʈ�� �浹 ��
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
            if (isEventStart) return; // �ߺ� Ȯ��

            isEventStart = true;

            SpawnBoss();

            // ���� ������ ����?

            StartCoroutine(EventTimerRoutine());

        }
        private void CheckEventCompletion()
        {
            // ������ �׾����� Ȯ�� (���� ������Ʈ�� �ı��Ǹ� null�� ��)
            if (!isBossDied && currentBoss == null)
            {
                isBossDied = true;
            }

            // Ÿ�̸ӿ� ���� óġ�� ��� �Ϸ�Ǹ� �ڷ����� Ȱ��ȭ
            if (isTimerFinished && isBossDied)
            {
                ActivateTeleporter();
            }
        }

        private void ActivateTeleporter()
        {
            isTeleporterActivated = true;

            // ���� ������ ����
        }

        private void SpawnBoss()
        {
            if (bossPrefab != null)
            {
                // ���� ������ġ�� �����Ǿ� ������ �ش� ��ġ�� ��ȯ �ƴϸ� �ڷ����� ��ġ�� ��ȯ
                Vector3 spawnPos = (bossSpawnPoint != null) ? bossSpawnPoint.position : transform.position;
                currentBoss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
            }
        }

        private IEnumerator EventTimerRoutine()
        {
            // 90�� Ÿ�̸� ����
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
