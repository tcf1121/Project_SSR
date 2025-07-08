using UnityEngine;

/// <summary>
/// ��ٸ� ����� ���� ���͸� ���� '�ƹ��͵� ���� �ʴ�' Ŭ�����Դϴ�.
/// NullReferenceException�� �����ϴ� ������ �մϴ�.
/// </summary>
public class NullClimber : IMonsterClimber
{
    public bool IsClimbing => false; // ��� ������ ����� �׻� '�ƴϿ�'��� ���մϴ�.

    public void Init(MonsterBrain brain) { } // �ʱ�ȭ�ص� �ƹ��͵� ���� �ʽ��ϴ�.

    public bool TryFindAndClimb(int dir, Vector2 playerPos) => false; // ����� �õ��ص� �׻� '����'��� ���մϴ�.
}