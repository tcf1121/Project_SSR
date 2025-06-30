using UnityEngine;
using System.Collections.Generic;
using System.Linq; // LINQ (FirstOrDefault) ����� ���� �ʿ�

namespace PHG
{
    [CreateAssetMenu(fileName = "AllMonsterStatData", menuName = "Samsara/All Monster Stat Data", order = 1)]
    public class AllMonsterStatData : ScriptableObject
    {
        [Tooltip("��� ���� Ÿ���� ���� ���")]
        public List<MonsterStatEntry> monsterStats = new List<MonsterStatEntry>();

        /// <summary>
        /// Ư�� ���� Ÿ���� ���� ��Ʈ���� ã�� ��ȯ�մϴ�.
        /// </summary>
        /// <param name="type">ã�� ���� Ÿ��</param>
        /// <returns>�ش� MonsterStatEntry ��ü (ã�� ������ ��� null)</returns>
        public MonsterStatEntry GetStatEntry(MonsterType type)
        {
            // LINQ�� FirstOrDefault�� ����Ͽ� ����Ʈ���� �ش� MonsterType�� ���� MonsterStatEntry�� ã���ϴ�.
            // ���� ���߿� ���� Ÿ���� ����~���鰳�� �ȴٸ�, �� List ��� Dictionary<MonsterType, MonsterStatEntry>�� ����Ͽ�
            // �� �� �ε��� �� O(1) �ð� ���⵵�� ������ ã�� �� �ֵ��� ����ȭ�ϴ� ���� ����� �� �ֽ��ϴ�.
            return monsterStats.FirstOrDefault(entry => entry.monsterType == type);
        }
    }
}