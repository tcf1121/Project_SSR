using UnityEngine;

namespace PHG
{
    public class RangedAttackBehavior : IAttackBehavior
    {
        RangeAttackState raState;
        public void Execute(MonsterBrain brain)
        {
           // Debug.Log(brain.name + " RangedAttackBehavior.Execute() 호출됨PHG");
            var target = brain.Target;
            if (target == null)
            {
                target = GameObject.FindWithTag("Player")?.transform;
                if (target == null)      // 씬에 플레이어가 아직 없다면
                {
                //    Debug.LogWarning("Player 없음 – 발사 스킵");
                    return;
                }
                brain.Target = target;   // 캐시
            }
            else
            {
                //Debug.Log("Passed FindwithTag Player-PHG2");
            }

                var prefab = brain.StatData.projectileprefab;
            var muzzle = brain.Muzzle;

           // Debug.Log($"[Execute] IsRanged={brain.IsRanged}");
            // 1단계: IsRanged 프로퍼티 접근 테스트
           // Debug.Log("1. IsRanged 확인 중...");
            bool isRanged = brain.IsRanged;
          //  Debug.Log($" => IsRanged 값: {isRanged}");

            // 2단계: prefab 변수 null 체크 테스트
          //  Debug.Log("2. prefab 확인 중...");
            bool isPrefabNull = (prefab == null);
           // Debug.Log($" => prefab은 null인가? {isPrefabNull}");

            // 3단계: muzzle 변수 null 체크 테스트
        //    Debug.Log("3. muzzle 확인 중...");
            bool isMuzzleNull = (muzzle == null);
         //   Debug.Log($" => muzzle은 null인가? {isMuzzleNull}");

         //   Debug.Log("--- 모든 개별 조건 확인 완료 ---");

            if (!isRanged || isPrefabNull || isMuzzleNull)
            {
           //     Debug.LogWarning("최종 조건: 미충족. 발사를 중단합니다.");
                return;
            }
            else
            {
           //     Debug.Log("최종 조건: 충족. 발사를 계속합니다.");
            }

            var pool = ProjectilePool.Instance;
            if (pool == null)
            {
        //        Debug.LogError("[RangedAttackBehavior] ProjectilePool.Instance가 null입니다! 씬에 ProjectilePool이 있는지, 초기화가 제대로 되는지 확인해주세요.");
                return;
            }
            else
            {
          //      Debug.Log("[RangedAttackBehavior] ProjectilePool.Instance 확인 완료.");
            }

                Vector2 aim = (Vector2)target.position + Vector2.up * 0.25f;
            Vector2 baseDir = (aim - (Vector2)brain.Muzzle.position).normalized;

           
          //  Debug.Log($"ProjectilePrefab: {prefab?.name ?? "NULL"}");
            float speed = brain.StatData.projectileSpeed;

            if (brain.StatData.firePattern == MonsterStatEntry.FirePattern.Single)
            {
           //     Debug.Log($"[RangedAttackBehavior] 단일 발사 로직 진입...");
                var p = ProjectilePool.Instance.Get(prefab, muzzle.position);
                if (p == null)
                {
           //         Debug.LogError("[RangedAttackBehavior] ProjectilePool.Get() 메서드가 null을 반환했습니다. 풀 로직(오브젝트 생성/반환 부분)을 확인해주세요.");

                    return;
                }
                else
                {
           //         Debug.Log($"[RangedAttackBehavior] 투사체 생성 및 발사 성공: {p.name}");
                }
                float angle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
                p.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                p.Launch(baseDir, speed);
            }
            else // Spread
            {
              //  Debug.Log($"[RangedAttackBehavior] 확산 발사 로직 진입...");
                int pellets = Mathf.Max(1, brain.StatData.pelletCount);
                float spread = brain.StatData.spreadAngle;
                float step = (pellets > 1) ? spread / (pellets - 1) : 0f;

                for (int i = 0; i < pellets; ++i)
                {
                    float offset = -spread * 0.5f + step * i;
                    float angle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg + offset;
                    Vector2 dir = Quaternion.AngleAxis(angle, Vector3.forward) * Vector2.right;

                    var p = ProjectilePool.Instance.Get(prefab, muzzle.position);
                    p.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                    p.Launch(dir, speed);
                }
            }
        }
    }
}