using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;

public class DangerIndexManager : MonoBehaviour
{
    public static DangerIndexManager Instance { get; private set; }

    // ���� ��ġ
    [SerializeField] private float DangerIndex = 0f;

    // UI
    [SerializeField] private TextMeshProUGUI dangerText;

    // �̺�Ʈ
    public event Action<float> OnDangerChanged;

    // ���̵� ���� Ÿ�̸�
    [SerializeField] private float timeStack = 0f; // ���� * 10�� ���� ��, ���� �ʸ� 10������ ������
    private float time = 10f;
    private float count = 0;

    private int stage = 0; // ���� ��������, 1���������� ��� = 0

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
            DangerUpdate(); // ���� ��������, �ð�
        }
    }

    /// <summary>
    /// �������� ����� ȣ��, �����ġ�� ���������� ���� ������
    /// </summary>
    /// <param name="stage">����� ��������</param>
    public void StageChangeDangerIndexSetting(int stage)
    {
        this.stage = stage - 1;
    }

    /// <summary>
    /// ���� ��ġ ���� �Լ�, �����Կ� ���� ���� ���� ui �� �̺�Ʈ�� �˸�
    /// </summary>
    public void DangerUpdate()
    {
        DangerIndex = (1.0f + 0.1012f * timeStack) * Mathf.Pow(1.15f, stage); // �Լ�
        UpdateUI();
        OnDangerChanged?.Invoke(DangerIndex);
    }

    /// <summary>
    /// Ÿ�� ���� �߰�
    /// </summary>
    public void AddtimeStack()
    {
        timeStack++;
    }

    /// <summary>
    /// UI ������Ʈ
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
    /// �����ġ �ʱ�ȭ �Լ� (���������� ���������)
    /// </summary>
    /// <param name="index">������ ��������</param>
    public void DangerIndexReset(int index)
    {
        DangerIndex = 0;
        timeStack = 0f;
        count = 0;
        StageChangeDangerIndexSetting(index);
    }
}