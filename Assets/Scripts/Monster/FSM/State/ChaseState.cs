using UnityEngine;


public class ChaseState : IState
{
    private Monster _monster;
    private readonly MonsterStatEntry _statData;

    private readonly IMonsterJumper jumper;
    private readonly IMonsterClimber climber; // IMonsterJumper 구현체

    private const float WALL_CHECK_DIST = 0.25f;
    private const float STUCK_VEL_TOL = 0.05f;
    private float stuckTime = 0f;
    private const float STUCK_DURATION = 1.0f;
    private float lastShot;

    public ChaseState(Monster monster)
    {
        _monster = monster;
        _statData = monster.Brain.StatData;
        jumper = _monster.Brain;                // IMonsterJumper 구현체
        climber = _monster.Brain.Climber;
    }

    /* ───────── IState ───────── */
    public void Enter()
    {

        if (_statData.hasIdleAnim)
            _monster.PlayAnim(AnimNames.Walk);


        lastShot = Time.time;
    }

    public void Tick()
    {
        if (!_monster.PlayerInRange(_statData.chaseRange))
        {
            _monster.Brain.ChangeState(StateID.Patrol);
            return;
        }

        /* --- 기본 벡터 및 판정 --- */
        int dir = _monster.LookAtPlayerDirection();

        bool grounded = _statData.enableJump ? jumper.IsGrounded()
                                      : CheckGroundedFallback();
        bool midJump = _statData.enableJump ? jumper.IsMidJump : false;

        bool isClimbing = _statData.enableLadderClimb ? climber.IsClimbing : false;


        if (!midJump && grounded && !isClimbing)
            Orient(dir);

        /* --- 벽/점프 처리 (점프 가능 개체만) --- */
        Vector2 rayDir = _monster.Transfrom.right * Mathf.Sign(_monster.Transfrom.localScale.x);
        RaycastHit2D wallCheck = Physics2D.Raycast(
            _monster.WallSensor.position, rayDir, WALL_CHECK_DIST,
            _statData.wallMask);
        if (grounded && Mathf.Abs(_monster.Rigid.velocity.x) < STUCK_VEL_TOL)
            stuckTime += Time.deltaTime;
        else
            stuckTime = 0f;
        bool wallAhead = wallCheck.collider != null;
        bool stuck = grounded && Mathf.Abs(_monster.Rigid.velocity.x) < STUCK_VEL_TOL;
        bool targetAbove = _monster.LookAtPlayerYPos() > 0.5f;
        bool targetNear = _monster.DistanceTarget() < 8f;
        bool cliffAhead = IsCliffAhead(dir);
        Debug.Log($"[{_monster.Brain.name}] 낭떠러지 감지: {cliffAhead} / 방향: {dir}");

        bool shouldJumpByWall = wallAhead && (stuck || targetAbove || targetNear);
        bool shouldJumpByCliff = cliffAhead && (targetAbove || targetNear);

        // 낭떠러지 감지 시 이동 중지 (점프 못하는 애들만)
        if (!_statData.enableJump)
        {
            int moveDir = dir; // ← 기존 방향
            cliffAhead = IsCliffAhead(moveDir);
            if (cliffAhead)
            {
                _monster.Rigid.velocity = new Vector2(0f, _monster.Rigid.velocity.y); // 정지
                return;
            }
        }
        if (_statData.enableJump &&
grounded && !midJump && jumper.ReadyToJump() &&
(shouldJumpByWall || shouldJumpByCliff))
        {

            if (_statData.hasIdleAnim)
                _monster.PlayAnim(AnimNames.Jump);

            float dy = Mathf.Abs(_monster.LookAtPlayerYPos());
            jumper.PerformJump(dir, dy,
                               _statData.jumpForce,
                               _statData.jumpHorizontalFactor,
                               _statData.jumpCooldown);
            return;
        }
        // 벽 감지가 안 되었지만 일정 시간 동안 이동이 없고 플레이어가 근처면 → 전진 점프
        //if (brain.CanJump &&
        //    grounded && !midJump && jumper.ReadyToJump() &&
        //    !wallAhead && stuckTime >= STUCK_DURATION && targetNear)
        //{
        //    float dy = Mathf.Abs(toPl.y);
        //    jumper.PerformJump(dir, dy,
        //                       statData.jumpForce,
        //                       statData.jumpHorizontalFactor,   //  wallSensor jump와 동일
        //                       statData.jumpCooldown);
        //    return;
        //}

        /* --- 수평 이동 --- */
        /* --- 추적 유지 구간: 수평 이동만 계속 --- */
        /* 사다리 AI */
        if (_statData.enableLadderClimb && Mathf.Abs(_monster.LookAtPlayerYPos()) > 1.25f)
            _monster.Brain.Climber?.TryFindAndClimb(dir);
        if (grounded && !midJump)
        {
            float chargeBoost =
                (!_statData.isRanged && _statData.isCharging && _monster.PlayerInRange(_statData.chargeRange))
                ? _statData.rushMultiplier
                : 1f;

            float targetX = dir * _monster.MonsterStats.MoveSpeed * chargeBoost;
            _monster.Rigid.velocity = new Vector2(targetX, _monster.Rigid.velocity.y);
        }
        /* 공중 유지속도(점프 중) */
        else if (!grounded && midJump)
            _monster.Rigid.velocity = new Vector2(_monster.Rigid.velocity.x, _monster.Rigid.velocity.y);

        /* 추적 종료 판정 */
        if (!_monster.PlayerInRange(_statData.chaseRange))
        {
            _monster.ChangeState(StateID.Patrol);
            return;
        }

        if (_statData.isRanged)
        {
            /* --- 공격 사거리 진입 시 전이 --- */
            if (_monster.PlayerInRange(_statData.attackRange))
            {
                _monster.Rigid.velocity = Vector2.zero;
                _monster.ChangeState(StateID.Attack);
                return;
            }
            /* 원거리형 발사 로직 */
            // ChaseState.cs → Tick() 내부에서 완전히 정리된 로직

            // 공격 사거리는 아니지만 추격 유지 범위 내에 있다면 → 이동만
            if (_monster.PlayerInRange(_statData.chaseRange))
            {
                float targetX = dir * _monster.MonsterStats.MoveSpeed;
                _monster.Rigid.velocity = new Vector2(targetX, _monster.Rigid.velocity.y);
                return;
            }
        }
        // 2b. 사격 대기 범위(readyRange) 진입 시 AimReady State로 전환
        // (공격 사거리 밖이지만 조준 준비 상태로 돌입)
        // statData.readyRange는 statData.attackRange보다 크고 statData.chaseRange보다 큼
        else if (!_statData.isRanged)
        {
            if (_monster.PlayerInRange(_statData.attackRange))
            {
                if (isClimbing) // 사다리 타고 있으면 공격 상태로 전환하지 않고 현재 상태 (Chase) 유지
                {
                    // Debug.Log($"[{brain.name}] 사다리 등반 중이라 공격 상태로 전환하지 않음.");
                    // 사다리 로직을 계속 실행하도록 return 하지 않음
                }
                else // 사다리 타고 있지 않으면 공격 상태로 전환
                {
                    _monster.Rigid.velocity = Vector2.zero;
                    _monster.ChangeState(StateID.Attack);
                    return; // 공격 상태로 전환했으니 이번 Tick은 여기서 종료
                }
            }
        }
    }
    private bool IsCliffAhead(int dir)
    {
        Vector2 origin = _monster.GroundSensor.position + Vector3.right * dir * 0.3f + Vector3.down * 0.1f;
        float radius = 0.1f;

#if UNITY_EDITOR
        Debug.DrawRay(origin, Vector2.down * 0.2f, Color.red, 0.2f);
#endif

        return !Physics2D.OverlapCircle(origin, radius, _statData.groundMask);
    }


    public void Exit()
    {
        if (_monster.Brain.IsGrounded() && !_monster.Brain.IsMidJump)
            _monster.Rigid.velocity = Vector2.zero;

    }

    /* ───────── Helper ───────── */
    private void Orient(int dir)
    {
        Vector3 s = _monster.Transfrom.localScale;
        s.x = Mathf.Abs(s.x) * dir;
        _monster.Transfrom.localScale = s;
    }

    private bool CheckGroundedFallback()
    {
        // sensor 기준 간단한 레이캐스트 – JumpMove 미사용 시 전용
        return Physics2D.Raycast(
                   _monster.GroundSensor.position,
                   Vector2.down,
                   0.15f,
                   _statData.groundMask);
    }

    private void Shoot()
    {
        if (_monster.MuzzlePoint == null) return;
        ProjectilePool pool = ProjectilePool.Instance;
        if (pool == null) return;

        Vector2 baseDir = (_monster.Target.position - _monster.MuzzlePoint.position).normalized;
        Projectile prefab = _statData.projectileprefab;

        if (_statData.firePattern == MonsterStatEntry.FirePattern.Single)
        {
            Projectile p = pool.Get(prefab, _monster.MuzzlePoint.position);
            p.transform.rotation = Quaternion.FromToRotation(Vector2.right, baseDir);
            p.Launch(baseDir, _statData.projectileSpeed, _monster.Brain);
        }
        else
        {
            int pellets = Mathf.Max(1, _statData.pelletCount);
            float spread = _statData.spreadAngle;
            float step = (pellets > 1) ? spread / (pellets - 1) : 0f;

            for (int i = 0; i < pellets; ++i)
            {
                float angle = -spread * 0.5f + step * i;
                Vector2 dirVec = Quaternion.AngleAxis(angle, Vector3.forward) * baseDir;

                Projectile p = pool.Get(prefab, _monster.MuzzlePoint.position);
                p.transform.rotation = Quaternion.FromToRotation(Vector2.right, dirVec);
                p.Launch(dirVec, _statData.projectileSpeed, _monster.Brain);
            }
        }
    }
}