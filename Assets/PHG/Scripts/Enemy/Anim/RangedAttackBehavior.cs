using UnityEngine;

namespace PHG
{
    public class RangedAttackBehavior : IAttackBehavior
    {
        RangeAttackState raState;
        public void Execute(MonsterBrain brain)
        {
            var target = brain.Target;
            if (target == null)
            {
                target = GameObject.FindWithTag("Player")?.transform;
                if (target == null)     // 씬에 플레이어가 아직 없다면
                {
                    return;
                }
                brain.Target = target;    // 캐시
            }
            else
            {
            }

            var prefab = brain.StatData.projectileprefab;
            var muzzle = brain.Muzzle;

            bool isRanged = brain.IsRanged;
            bool isPrefabNull = (prefab == null);
            bool isMuzzleNull = (muzzle == null);

            if (!isRanged || isPrefabNull || isMuzzleNull)
            {
                return;
            }
            else
            {
            }

            var pool = ProjectilePool.Instance;
            if (pool == null)
            {
                return;
            }
            else
            {
            }

            Vector2 aim = (Vector2)target.position + Vector2.up * 0.25f;
            Vector2 baseDir = (aim - (Vector2)brain.Muzzle.position).normalized;

            float speed = brain.StatData.projectileSpeed;

            if (brain.StatData.firePattern == MonsterStatEntry.FirePattern.Single)
            {
                var p = ProjectilePool.Instance.Get(prefab, muzzle.position);
                if (p == null)
                {
                    return;
                }
                else
                {
                }
                float angle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
                p.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                p.Launch(baseDir, speed, brain);
            }
            else // Spread
            {
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
                    p.Launch(dir, speed, brain);
                }
            }
        }
    }
}