using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BODController : MonoBehaviour
{
    [Header("스탯")]
    public float maxHP = 3000;
    private float currentHP;
    public float attackPower = 60;
    public float defense = 50;
    public float moveSpeed = 7f;
    // 레벨당 추가체력 +80
    // 레벨당 추가 데미지 +7

    [Header("감지 및 공격 범위")]
    public float normalAttackRange = 3f;
    public float skillRange = 7f;

    [Header("일반 공격 설정")]
    public float normalAttackCooldown = 2f;
}
