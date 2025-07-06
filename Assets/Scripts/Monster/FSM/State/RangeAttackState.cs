using UnityEngine;

/// <summary>
/// 원거리 몬스터 – 공격/조준/추격 제어
/// </summary>
public class RangeAttackState : IState
{
    /* ───────── refs ───────── */
    readonly private Monster _monster;
    readonly private MonsterStatEntry _statData;
    private float lastShot;

    const float AirAccel = 8f;

    public RangeAttackState(Monster monster)
    {
        _monster = monster;
        _statData = monster.Brain.StatData;
    }

    public void Enter()
    {
        if (_statData.hasIdleAnim)
            _monster.PlayAnim(AnimNames.Attack);


        lastShot = Time.time;
    }

    public void Tick()
    {
        // 1. 쿨타임 확인
        if (Time.time - lastShot < _statData.rangedCooldown)
            return;

        // 2. 플레이어 유효성 및 거리 체크

        if (!_monster.PlayerInRange(_statData.chaseRange))
        {

            // brain.ChangeState(StateID.Chase);
            return;
        }
        if (!_monster.PlayerInRange(_statData.attackRange))
        {
            _monster.ChangeState(StateID.Chase);
            return;
        }

        FacePlayer();
        // 3. 공격 범위 안이면 정지 후 애니메이션만 재생
        if (_monster.Brain.IsGrounded() && !_monster.Brain.IsMidJump)
            _monster.Rigid.velocity = Vector2.zero;

        // Execute() 직접 호출 삭제!
        _monster.Animator.Play(AnimNames.Attack, 0, 0f);

        // 4. 쿨타임 초기화
        lastShot = Time.time;
    }

    public void Exit()
    {
        if (_monster.Brain.IsGrounded() && !_monster.Brain.IsMidJump)
            _monster.Rigid.velocity = Vector2.zero;

    }
    void Shoot()
    {
        if (_monster.MuzzlePoint == null) return;
        ProjectilePool pool = ProjectilePool.Instance;
        if (pool == null)
        {
            Debug.LogError("ProjectilePool is null");
            return;
        }

        // 조준 위치 보정 (Y값 내려서 상체나 머리 쪽 조준)
        Vector2 aimTarget = _monster.Target.position + Vector3.up * 0.25f;  // ← 보정값 필요 시 조절
        Vector2 baseDir = (aimTarget - (Vector2)_monster.MuzzlePoint.position).normalized;

        // 기본 투사체
        Projectile prefab = _statData.projectileprefab;

        if (_statData.firePattern == MonsterStatEntry.FirePattern.Single)
        {
            Projectile p = pool.Get(prefab, _monster.MuzzlePoint.position);

            // 회전은 Z축 기준 2D 전용으로 적용
            float angle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
            p.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            p.Launch(baseDir, _statData.projectileSpeed, _monster.Brain);
        }
        else // Spread
        {
            int pellets = Mathf.Max(1, _statData.pelletCount);
            float spread = _statData.spreadAngle;
            float step = (pellets > 1) ? spread / (pellets - 1) : 0f;

            for (int i = 0; i < pellets; ++i)
            {
                float angleOffset = -spread * 0.5f + step * i;
                float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
                float shotAngle = baseAngle + angleOffset;

                Vector2 dir = Quaternion.AngleAxis(shotAngle, Vector3.forward) * Vector2.right;

                Projectile p = pool.Get(prefab, _monster.MuzzlePoint.position);
                p.transform.rotation = Quaternion.AngleAxis(shotAngle, Vector3.forward);
                p.Launch(dir, _statData.projectileSpeed, _monster.Brain);
            }
        }
    }


    void FacePlayer()
    {
        if (_monster.Target == null) return;
        int sign = _monster.LookAtPlayerDirection();
        Vector3 sc = _monster.Transfrom.localScale;
        sc.x = Mathf.Abs(sc.x) * sign;
        _monster.Transfrom.localScale = sc;
    }
}