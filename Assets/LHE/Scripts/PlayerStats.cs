using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("�ɸ��� ����")]
    public int level = 1;
    public float currenExp = 30;
    public float reqExp; // �ʿ� ����ġ
    public float money;

    [Header("�ɸ��� ����")]
    public float maximumHp;
    public float currentHp;
    public float atk;
    public float hpRegen;
    public float Speed = 7f;
    public float jump = 1f;


    public void HPStatsUpdate()
    {
        maximumHp = level * 33f + 67f;
    }

    public void ATKStatsUpdate()
    {
        atk = level * 2.5f + 7.5f;
    }

    public void HpRegenStatsUpdate()
    {
        hpRegen = level * 0.2f + 0.8f;
    }

    public void MoveSpeedStatsUpdate()
    {
        // Speed = �ܺ� ��ҿ� ���� ����

    }

    public void JumpForceStatsUpdate()
    {
        // jump = �ܺο�ҿ� ���� ����
        // 0���� �۾��� �� ���� (���������� �ƴ��̻�)
    }
}

