using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;

public class DangerIndexManager : MonoBehaviour
{
    public static DangerIndexManager Instance { get; private set; }

    // 위험 수치
    [SerializeField] private float DangerIndex = 0f;

    // UI
    [SerializeField] private TextMeshProUGUI dangerText;

    // 이벤트
    public event Action<float> OnDangerChanged;

    // 난이도 증가 타이머
    [SerializeField] private float timeStack = 0f; // 스택 * 10이 현재 초, 현재 초를 10단위로 나눈것
    private float time = 10f;
    private float count = 0;

    private int stage = 0; // 현재 스테이지, 1스테이지의 경우 = 0

    // Get
    public float GetDangerIndex() => DangerIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        count += Time.deltaTime;

        if (count >= time)
        {
            count -= time;
            AddtimeStack();
            DangerUpdate(); // 현재 스테이지, 시간
        }
    }

    /// <summary>
    /// 스테이지 변경시 호출, 위험수치를 스테이지에 따라 세팅함
    /// </summary>
    /// <param name="stage">변경될 스테이지</param>
    public void StageChangeDangerIndexSetting(int stage)
    {
        this.stage = stage - 1;
    }

    /// <summary>
    /// 위험 수치 증가 함수, 증가함에 따라 변경 내용 ui 및 이벤트로 알림
    /// </summary>
    public void DangerUpdate()
    {
        DangerIndex = (1.0f + 0.1012f * timeStack) * Mathf.Pow(1.15f, stage); // 함수
        UpdateUI();
        OnDangerChanged?.Invoke(DangerIndex);
    }

    /// <summary>
    /// 타임 스택 추가
    /// </summary>
    public void AddtimeStack()
    {
        timeStack++;
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        if (dangerText != null)
        {
            int Index = (int)Mathf.Round(DangerIndex * 10);
            dangerText.text = $"{Index * 0.1}";
        }
    }

    /// <summary>
    /// 위험수치 초기화 함수 (스테이지는 정해줘야함)
    /// </summary>
    /// <param name="index">리셋할 스테이지</param>
    public void DangerIndexReset(int index)
    {
        DangerIndex = 0;
        timeStack = 0f;
        count = 0;
        StageChangeDangerIndexSetting(index);
    }
}