using UnityEngine;
using System.Collections.Generic;
using System.Linq; // LINQ (FirstOrDefault) 사용을 위해 필요

namespace PHG
{
    [CreateAssetMenu(fileName = "AllMonsterStatData", menuName = "Samsara/All Monster Stat Data", order = 1)]
    public class AllMonsterStatData : ScriptableObject
    {
        [Tooltip("모든 몬스터 타입의 스탯 목록")]
        public List<MonsterStatEntry> monsterStats = new List<MonsterStatEntry>();

        /// <summary>
        /// 특정 몬스터 타입의 스탯 엔트리를 찾아 반환합니다.
        /// </summary>
        /// <param name="type">찾을 몬스터 타입</param>
        /// <returns>해당 MonsterStatEntry 객체 (찾지 못했을 경우 null)</returns>
        public MonsterStatEntry GetStatEntry(MonsterType type)
        {
            // LINQ의 FirstOrDefault를 사용하여 리스트에서 해당 MonsterType을 가진 MonsterStatEntry를 찾습니다.
            // 만약 나중에 몬스터 타입이 수십~수백개가 된다면, 이 List 대신 Dictionary<MonsterType, MonsterStatEntry>를 사용하여
            // 한 번 로드한 후 O(1) 시간 복잡도로 빠르게 찾을 수 있도록 최적화하는 것을 고려할 수 있습니다.
            return monsterStats.FirstOrDefault(entry => entry.monsterType == type);
        }
    }
}