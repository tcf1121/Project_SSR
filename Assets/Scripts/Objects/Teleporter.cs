using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Teleporter : MonoBehaviour
{
    [Header("이벤트")]
    [SerializeField] Transform bossSpawnPoint; // 보스 스폰 위치
    [SerializeField] private float eventDuration = 90f; // 이벤트 지속 시간


    private bool isEventStart = false; // 이벤트 시작 여부
    private bool isTimerFinished = false; // 90초 타이머가 끝났는지
    private bool isBossDied = false; // 보스 사망여부
    private bool isTeleporterActivated = false; // 텔레포터가 다음 스테이지로 넘어갈 준비가 되었는지

    private GameObject currentBoss; // 소환된 보스

    public UnityAction IsDie { get => isDie; }
    private UnityAction isDie;
    private UnityAction isClear;
    private BoxCollider2D _collider;
    [SerializeField] private Animator _animator;
    [SerializeField] private GameObject _TimeLeftUI;
    [SerializeField] private Image _timeFill;

    private void Awake()
    {
        if (bossSpawnPoint == null)
        {
            bossSpawnPoint = this.transform;
        }
        _collider = GetComponent<BoxCollider2D>();
        isDie += KillBoss;
        isClear += CheckClear;
    }

    public void OnCollider()
    {
        _collider.enabled = true;
    }

    public void OffCollider()
    {
        _collider.enabled = false;
    }

    public void ActivateTelePort()
    {
        if (!isEventStart)
        {
            StartTeleporterEvent();
        }

        else if (isTeleporterActivated)
        {
            LoadNextScene();
        }
    }

    private void StartTeleporterEvent()
    {
        if (isEventStart) return; // 중복 확인

        isEventStart = true;
        _animator.SetBool("Activate", true);
        SpawnBoss();

        // 몬스터 스포너 가속?

        StartCoroutine(EventTimerRoutine());
        Debug.Log("텔레포터 활성화");

    }

    private void KillBoss()
    {
        isBossDied = true;
        _animator.SetTrigger("KillBoss");
        isClear?.Invoke();
    }

    private void CheckClear()
    {
        if (isTimerFinished && isBossDied)
        {
            _animator.SetBool("IsClear", true);
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
        // if (bossPrefab != null)
        // {
        //     // 보스 스폰위치가 설정되어 있으면 해당 위치에 소환 아니면 텔레포터 위치에 소환
        //     Vector3 spawnPos = (bossSpawnPoint != null) ? bossSpawnPoint.position : transform.position;
        //     currentBoss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        // }
    }

    private IEnumerator EventTimerRoutine()
    {
        _TimeLeftUI.SetActive(true);
        float currentTime = eventDuration;// 90초 타이머 시작
        while (currentTime > 0.0f)
        {
            currentTime -= Time.deltaTime;
            _timeFill.fillAmount = (eventDuration - currentTime) / eventDuration;
            yield return new WaitForFixedUpdate();
        }
        isTimerFinished = true;
        isClear?.Invoke();
    }

    private void LoadNextScene()
    {
        GameManager.StageClear();
        GameManager.NextStage();
    }

}
