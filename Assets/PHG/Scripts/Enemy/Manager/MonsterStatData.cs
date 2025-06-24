using UnityEngine;

[CreateAssetMenu(fileName = "MonsterStatData", menuName = "Samsara/Monster Stat Data", order = 1)]
public class MonsterStatData : ScriptableObject
{
    [Header("�⺻ ����")]
    public int maxHP = 100;
    public int damage = 10;
    public float moveSpeed = 2f;

    [Header("AI �ν� ����")]
    public float patrolRange = 3f;
    public float chaseRange = 6f;
    public float attackRange = 1f;
}