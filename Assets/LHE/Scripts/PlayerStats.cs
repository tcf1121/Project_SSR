using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("케릭터 상태")]
    public int level = 1;
    public float currenExp = 30;
    public float reqExp; // 필요 경험치
    public float money;

    [Header("케릭터 스탯")]
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
        // Speed = 외부 요소에 의한 변경

    }

    public void JumpForceStatsUpdate()
    {
        // jump = 외부요소에 의한 변경
        // 0보다 작아질 수 없음 (점프금지가 아닌이상)
    }
}

