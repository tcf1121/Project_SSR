using UnityEngine;
using System.Collections;
using PHG;

public class ChaseState : IState
{
    private Monster _monster;
    private readonly MonsterStatEntry _statData;

    private readonly IMonsterJumper jumper;
    private readonly IMonsterClimber climber;

    private const float WALL_CHECK_DIST = 0.25f;
    private const float STUCK_VEL_TOL = 0.05f;
    private float stuckTime = 0f;
    private const float STUCK_DURATION = 1.0f;
    private float lastShot;

    private Collider2D[] _overlapResults = new Collider2D[5];

    public ChaseState(Monster monster)
    {
        _monster = monster;
        _statData = monster.Brain.StatData;
        jumper = _monster.Brain;
        climber = _monster.Brain.Climber;
    }

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
            Debug.Log("상태변환: ChaseState → PatrolState");
            _monster.Brain.ChangeState(StateID.Patrol);
            return;
        }

        int dir = _monster.LookAtPlayerDirection();

        bool grounded = _statData.enableJump ? jumper.IsGrounded() : CheckGroundedFallback();
        bool midJump = _statData.enableJump ? jumper.IsMidJump : false;
        bool isClimbing = _statData.enableLadderClimb ? climber.IsClimbing : false;


        if (!midJump && grounded && !isClimbing)
            Orient(dir);

        Vector2 rayDir = _monster.Transfrom.right * Mathf.Sign(_monster.Transfrom.localScale.x);
        RaycastHit2D wallCheck = Physics2D.Raycast(
            _monster.WallSensor.position, rayDir, WALL_CHECK_DIST,
            _statData.wallMask);

        // Stuck 시간 계산
        // 공중에 있을 때는 stuckTime을 계산하지 않습니다. (바닥에 붙어있을 때만 끼임 체크)
        if (grounded && Mathf.Abs(_monster.Rigid.velocity.x) < STUCK_VEL_TOL && Mathf.Abs(_monster.Rigid.velocity.y) < STUCK_VEL_TOL)
            stuckTime += Time.deltaTime;
        else
            stuckTime = 0f;

        bool wallAhead = wallCheck.collider != null;
        // trulyStuck 상태는 벽에 붙어있으면서 움직임이 없는 경우에만 계산됩니다.
        bool trulyStuck = grounded && wallAhead && stuckTime >= STUCK_DURATION;
        bool targetAbove = _monster.LookAtPlayerYPos() > 0.5f;
        bool targetNear = _monster.DistanceTarget() < 8f;
        bool cliffAhead = IsCliffAhead(dir);

        // 점프가 불가능한 몬스터가 낭떠러지 앞에 있다면 이동 중지
        if (!_statData.enableJump && cliffAhead)
        {
            _monster.Rigid.velocity = new Vector2(0f, _monster.Rigid.velocity.y); // 수평 속도만 0으로
            return; // 낭떠러지 앞에서 멈춤
        }

        // 진정한 Stuck 상태일 때 Un-stick 시도
        if (trulyStuck)
        {
            AttemptUnstuck();
            // AttemptUnstuck이 성공하면 stuckTime이 초기화됩니다.
            return; // 이번 프레임에 Unstuck이 발생했으면 추가 이동은 하지 않습니다.
        }

        // Refined jump conditions
        bool shouldPerformJump = false;
        float jumpHeightParameter = 0f;

        // Case 1: 낭떠러지를 피하기 위한 점프 (플레이어 위치와 상관없이)
        if (cliffAhead && _statData.enableJump && grounded && !midJump && jumper.ReadyToJump())
        {
            shouldPerformJump = true;
            jumpHeightParameter = targetAbove ? _monster.LookAtPlayerYPos() : _statData.jumpForce * 0.15f;
        }
        // Case 2: 벽에 막혔을 때 또는 플레이어를 추격하기 위한 점프 (플레이어가 위에 있는 경우)
        else if ((wallAhead || trulyStuck) && _statData.enableJump && grounded && !midJump && jumper.ReadyToJump())
        {
            if (targetAbove)
            {
                shouldPerformJump = true;
                jumpHeightParameter = _monster.LookAtPlayerYPos();
            }
            else if (trulyStuck && targetNear && !targetAbove && _monster.LookAtPlayerYPos() >= -0.5f)
            {
                shouldPerformJump = true;
                jumpHeightParameter = _statData.jumpForce * 0.1f;
            }
        }

        if (shouldPerformJump)
        {
            if (_statData.hasIdleAnim)
                _monster.PlayAnim(AnimNames.Jump);

            jumper.PerformJump(dir, jumpHeightParameter,
                               _statData.jumpForce,
                               _statData.jumpHorizontalFactor,
                               _statData.jumpCooldown);
            return; // Jump initiated, end Tick for this frame
        }

        /* --- 수평 이동 --- */
        // 공중에 있지 않고 (grounded) 점프 중이 아닐 때 (midJump 아님) 또는
        // 공중에 있지만 점프 중일 때 (midJump) 수평 이동 적용
        // 핵심: 공중에서 단순히 낙하 중일 때도 (즉, grounded=false, midJump=false)
        // 수평 이동 로직이 적용되어야 합니다.
        if (!isClimbing) // 사다리 등반 중이 아닐 때만 수평 이동
        {
            float chargeBoost =
                (!_statData.isRanged && _statData.isCharging && _monster.PlayerInRange(_statData.chargeRange))
                ? _statData.rushMultiplier
                : 1f;

            float targetX = dir * _monster.MonsterStats.MoveSpeed * chargeBoost;
            _monster.Rigid.velocity = new Vector2(targetX, _monster.Rigid.velocity.y);
        }


        /* 사다리 AI */
        // 점프 중에는 사다리 등반 시도하지 않고, 지면에 착지했으며, 수직 속도가 거의 없을 때만 시도
        // (IsMidJump가 false이고, 현재 공중에 있지만 점프가 끝나서 낙하 중인 경우 포함)
        if (_statData.enableLadderClimb && !midJump && !isClimbing &&
            Mathf.Abs(_monster.LookAtPlayerYPos()) > 1.25f &&
            Mathf.Abs(_monster.Rigid.velocity.y) < 0.1f)
        {
            _monster.Brain.Climber?.TryFindAndClimb(dir,_monster.Target.position);
        }

        if (_monster.Brain.StatData.enableLadderClimb && isClimbing)
        {
            // 사다리 타고 있는 동안에는 수평 이동을 하지 않습니다.
            _monster.Rigid.velocity = new Vector2(0f, _monster.Rigid.velocity.y);
            return; // 사다리 등반 중이므로 여기서 Tick 종료
        }

        /* 추적 종료 판정 */
        if (!_monster.PlayerInRange(_statData.chaseRange))
        {
            Debug.Log($"{_monster.name}상태변환: ChaseState → PatrolState");
            // usePatrol 플래그는 여기서 직접 사용하지 않고, MonsterBrain의 ChangeState 메서드에서 처리하도록 맡깁니다.
            _monster.ChangeState(StateID.Patrol);
            Debug.Log($"{_monster.name}상태변환: ChaseState → PatrolState 전이 완료");
            return;
        }

        if (_statData.isRanged)
        {
            /* --- 공격 사거리 진입 시 전이 --- */
            if (_monster.PlayerInRange(_statData.attackRange))
            {
                _monster.Rigid.velocity = Vector2.zero; // 공격 준비를 위해 멈춤
                _monster.ChangeState(StateID.Attack);
                return;
            }
            // 이전에 있던 불필요한 중복 수평 이동 로직 제거 (위의 수평 이동 로직으로 통합)
        }
        else // 근접 몬스터
        {
            if (_monster.PlayerInRange(_statData.attackRange))
            {
                if (isClimbing)
                {
                    // 사다리 등반 중에는 공격 상태로 전환하지 않음 (계속 사다리 로직 수행)
                }
                else
                {
                    _monster.Rigid.velocity = Vector2.zero; // 공격 준비를 위해 멈춤
                    _monster.ChangeState(StateID.Attack);
                    return;
                }
            }
        }
    }

    // 추가: 몬스터가 끼었을 때 벗어나게 하는 메서드
    private void AttemptUnstuck()
    {
        Vector2 checkPosition = _monster.HitBox.bounds.center;
        Vector2 checkSize = _monster.HitBox.size * 0.99f;

        LayerMask obstacleMask = _statData.groundMask | _statData.wallMask;

        int numColliders = Physics2D.OverlapBoxNonAlloc(checkPosition, checkSize, 0f, _overlapResults, obstacleMask);

        bool moved = false;
        for (int i = 0; i < numColliders; i++)
        {
            Collider2D hitCol = _overlapResults[i];

            if (hitCol == null || hitCol == _monster.HitBox || hitCol.transform.root == _monster.transform.root) continue;

            // 겹친 콜라이더에서 벗어나는 방향으로 이동
            Vector2 displacement = (Vector2)_monster.transform.position - hitCol.ClosestPoint(_monster.transform.position);

            // X축 방향으로 밀어내기 (현재 이동 방향의 반대 방향으로)
            // Y축으로도 살짝 올릴 수 있지만, 대부분 벽 끼임은 X축 문제
            Vector3 pushDirection = new Vector3(-Mathf.Sign(_monster.transform.localScale.x), 0.5f, 0f).normalized;
            float pushMagnitude = 0.3f; // 밀어내는 힘 조절

            _monster.Rigid.simulated = false; // 물리 시뮬레이션 잠시 비활성화
            _monster.transform.position += pushDirection * pushMagnitude;
            _monster.Rigid.simulated = true;

            Debug.Log($"[{_monster.name}] Stuck! Attempting to un-stick from {hitCol.name}. Pushed.");
            moved = true;
            break;
        }

        if (moved)
        {
            stuckTime = 0f;
            _monster.Rigid.velocity = Vector2.zero; // 이동 후 속도 초기화 (안정화)
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
        // 낙하 중이거나 점프 중일 때는 velocity.zero를 강제하지 않습니다.
    }

    private void Orient(int dir)
    {
        Vector3 s = _monster.Transfrom.localScale;
        s.x = Mathf.Abs(s.x) * dir;
        _monster.Transfrom.localScale = s;
    }

    private bool CheckGroundedFallback()
    {
        return Physics2D.Raycast(
                       _monster.GroundSensor.position,
                       Vector2.down,
                       0.15f,
                       _statData.groundMask);
    }

    // Shoot 메서드는 변경 없음
    private void Shoot() { /* ... */ }
}