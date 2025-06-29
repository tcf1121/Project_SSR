using System; // TimeSpan 등의 유틸리티가 사용될 수 있으므로 유지
using UnityEngine;

namespace PHG
{
    public class FloatChaseState : IState
    {
        private readonly MonsterBrain brain;
        private readonly Rigidbody2D rb;
        private readonly Transform tf;
        private static Transform sPlayer; // 플레이어 위치 캐싱
        private readonly MonsterStatEntry statData; // MonsterStatData -> MonsterStatEntry로 변경

        /*원거리 공격용*/
        private readonly bool isRanged;
        Transform muzzle;
        float lastShot;

        public FloatChaseState(MonsterBrain brain)
        {
            this.brain = brain;
            rb = brain.rb; // brain.GetComponent<Rigidbody2D>() -> brain.rb로 변경
            tf = brain.tf; // brain.transform -> brain.tf로 변경
            statData = brain.statData; // brain.StatData -> brain.statData로 변경 (프로퍼티 이름 변경)

            // 원거리 몬스터 여부 확인 및 총구(muzzle) 초기화
            isRanged = brain.GetComponent<RangedTag>() != null;
            if (isRanged)
            {
                muzzle = tf.Find("MuzzlePoint");
            }
            // else { isRanged = false; } 이 부분은 불필요하므로 제거 (기본값 false)
        }

        public void Enter()
        {
            rb.gravityScale = 0f; // 비행 몬스터는 중력 없음
            rb.velocity = Vector2.zero; // 초기 속도 0
            if (sPlayer == null)
                sPlayer = GameObject.FindWithTag("Player")?.transform;
            lastShot = Time.time; // 공격 쿨타임 초기화
        }

        public void Tick()
        {
            // 플레이어 캐싱 (Tick에서 다시 확인)
            if (sPlayer == null)
            {
                sPlayer = GameObject.FindWithTag("Player")?.transform;
                if (sPlayer == null)
                    return; // 플레이어를 찾지 못하면 아무것도 하지 않음
            }

            Vector2 currentPos = tf.position;
            Vector3 targetPos3D = sPlayer.position; // Vector3로 유지
            // ★★★ 수정: Vector3를 Vector2로 명시적 형변환 ★★★
            Vector2 toPl = (Vector2)targetPos3D - currentPos;
            float dist = toPl.magnitude;

            // FSM 전환: 추격 범위를 벗어나면 Idle 상태로 전환 (비행 몬스터는 Patrol 없음)
            if (dist > statData.chaseRange * 1.5f) // statData.chaseRange 직접 참조 (여기서 * 1.5f는 예시)
            {
                brain.ChangeState(StateID.Idle);
                return;
            }

            /* ───────── ① 원거리 공격 로직 ───────── */
            if (isRanged)
            {
                // 원거리 공격 범위 내에 있고 쿨타임이 지나면 공격
                if (dist <= statData.attackRange && Time.time - lastShot >= statData.rangedCooldown) // statData 직접 참조
                {
                    Shoot();
                    lastShot = Time.time;
                }
                // 사격 대기 범위(readyRange) 밖이면 추격, 안이면 정지 또는 거리 유지
                if (dist > statData.readyRange) // statData.readyRange 직접 참조
                {
                    Vector2 dir = toPl.normalized;
                    rb.velocity = dir * statData.moveSpeed; // statData.moveSpeed 직접 참조
                    Orient(dir);
                }
                else
                {
                    rb.velocity = Vector2.zero; // 사격 대기 범위 내에 있으면 정지
                }
                return; // 원거리형 로직 처리 후 근접형 로직은 스킵
            }
            /* ───────── ② 이하 = 근거리/일반 추적 로직 ───────── */

            // 추격 범위 내에서 플레이어에게로 이동
            if (dist <= statData.chaseRange) // statData.chaseRange 직접 참조
            {
                // ★★★ 수정: Vector3를 Vector2로 명시적 형변환 ★★★
                Vector2 dir = ((Vector2)targetPos3D - currentPos).normalized;
                // 돌진 범위 내이면 돌진 가속도 적용
                float speed = statData.moveSpeed * (dist < statData.chargeRange ? statData.rushMultiplier : 1f); // statData 직접 참조
                rb.velocity = dir * speed;

                if (Mathf.Abs(dir.x) > 0.05f) // X축 이동이 있을 때만 방향 전환
                {
                    Vector3 sc = tf.localScale;
                    sc.x = Mathf.Abs(sc.x) * Mathf.Sign(dir.x);   // 부호 통일 (오른쪽 = 양수)
                    tf.localScale = sc;
                }
            }
            else // 추격 범위를 벗어나면 부유 (정지)
            {
                // 부유 효과 (위아래로 살짝 움직이는 애니메이션)
                float bobY = Mathf.Sin(Time.time * 1.5f) * 0.2f;
                rb.velocity = new Vector2(0f, bobY);
            }
        }

        public void Exit()
        {
            rb.velocity = Vector2.zero; // 상태 종료 시 속도 초기화
        }

        /* -------------------------------------------------------------- */
        #region helpers
        void Shoot()
        {
            if (muzzle == null) return;
            ProjectilePool pool = ProjectilePool.Instance;
            if (pool == null) { Debug.LogError("ProjectilePool is null"); return; }

            Vector2 baseDir = (sPlayer.position - muzzle.position).normalized;
            Projectile prefab = statData.projectileprefab; // statData.projectileprefab 직접 참조

            if (statData.firePattern == MonsterStatEntry.FirePattern.Single)
            {
                Projectile p = pool.Get(prefab, muzzle.position);
                p.transform.rotation = Quaternion.FromToRotation(Vector2.right, baseDir);
                p.Launch(baseDir, statData.projectileSpeed);
            }
            else // Spread 패턴
            {
                int pellets = Mathf.Max(1, statData.pelletCount);
                float spread = statData.spreadAngle;
                float step = pellets > 1 ? spread / (pellets - 1) : 0f;

                for (int i = 0; i < pellets; ++i)
                {
                    float angle = -spread * 0.5f + step * i;
                    Vector2 dir = Quaternion.AngleAxis(angle, Vector3.forward) * baseDir;

                    Projectile p = pool.Get(prefab, muzzle.position);
                    p.transform.rotation = Quaternion.FromToRotation(Vector2.right, dir);
                    p.Launch(dir, statData.projectileSpeed);
                }
            }
        }

        void Orient(Vector2 dir)
        {
            // X축 방향으로만 스케일 조정 (Y축은 건드리지 않음)
            if (Mathf.Abs(dir.x) > 0.05f) // X축 이동이 있을 때만 방향 전환
            {
                Vector3 sc = tf.localScale;
                sc.x = Mathf.Abs(sc.x) * Mathf.Sign(dir.x); // 오른쪽 = 양수
                tf.localScale = sc;
            }
        }
        #endregion
    }
}