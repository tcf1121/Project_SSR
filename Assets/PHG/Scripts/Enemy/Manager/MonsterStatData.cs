using UnityEngine;

[CreateAssetMenu(fileName = "MonsterStatData", menuName = "Samsara/Monster Stat Data", order = 1)]
public class MonsterStatData : ScriptableObject
{
    [Header("기본 스탯")]
    public int maxHP = 100;
    public int damage = 10;
    public float moveSpeed = 2f;

    [Header("AI 인식 범위")]
    public float patrolRange = 3f;
    public float chaseRange = 6f;
    public float attackRange = 1f;
}