using UnityEngine;
using System.Collections;

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

    public ChaseState(Monster monster)
    {
        _monster = monster;
        _statData = monster.Brain.StatData;
        jumper = _monster.Brain;
        climber = _monster.Brain.Climber; // MonsterBrain이 null이 아닌 객체를 보장
    }

    public void Enter()
    {
        if (_statData.hasIdleAnim)
            _monster.PlayAnim(AnimNames.Walk);
        stuckTime = 0f;
    }

    public void Tick()
    {
        // 1. 상태 전환 조건 우선 체크 (범위 이탈, 공격)
        if (!_monster.PlayerInRange(_statData.chaseRange))
        {
            _monster.Brain.ChangeState(StateID.Patrol);
            return;
        }

        // Null 체크 없이 안전하게 climber.IsClimbing 사용
        if (_monster.PlayerInRange(_statData.attackRange) && !climber.IsClimbing)
        {
            _monster.Rigid.velocity = Vector2.zero;
            _monster.Brain.ChangeState(StateID.Attack);
            return;
        }

        // 2. 사다리 등반 중이면 모든 추격 로직 중단
        // (사다리 기능 없는 몬스터는 climber.IsClimbing이 항상 false라 이 조건에 안 걸림)
        if (climber.IsClimbing)
        {
            return;
        }

        // 3. 추격 로직 실행
        int dir = _monster.LookAtPlayerDirection();
        Vector2 playerPos = _monster.Target.position;

        bool grounded = jumper.IsGrounded();
        bool midJump = jumper.IsMidJump;

        if (!midJump && grounded)
            Orient(dir);

        // --- 벽, 절벽, 끼임 상태 체크 ---
        RaycastHit2D wallCheck = Physics2D.Raycast(_monster.WallSensor.position, _monster.transform.right * dir, WALL_CHECK_DIST, _statData.wallMask);
        bool wallAhead = wallCheck.collider != null;
        bool cliffAhead = IsCliffAhead(dir);

        if (grounded && Mathf.Abs(_monster.Rigid.velocity.x) < STUCK_VEL_TOL)
            stuckTime += Time.deltaTime;
        else
            stuckTime = 0f;

        bool trulyStuck = grounded && wallAhead && stuckTime >= STUCK_DURATION;

        // --- 행동 결정 (끼임 탈출 > 사다리 > 점프 > 기본 이동) ---
        if (trulyStuck)
        {
            AttemptUnstuckSafer(dir);
            return;
        }

        // 사다리 탐색 및 등반 시도
        // (사다리 기능 없는 몬스터는 climber.TryFindAndClimb가 항상 false라 이 조건에 안 걸림)
        if (_statData.enableLadderClimb && !midJump)
        {
            if (climber.TryFindAndClimb(dir, playerPos))
            {
                return;
            }
        }

        // --- 점프 로직 ---
        if (_statData.enableJump)
        {
            bool targetAbove = playerPos.y > _monster.transform.position.y + 0.5f;
            if ((cliffAhead || (wallAhead && targetAbove)) && grounded && !midJump && jumper.ReadyToJump())
            {
                if (_statData.hasIdleAnim) _monster.PlayAnim(AnimNames.Jump);
                float jumpHeight = targetAbove ? _monster.LookAtPlayerYPos() : _statData.jumpForce * 0.15f;
                jumper.PerformJump(dir, jumpHeight, _statData.jumpForce, _statData.jumpHorizontalFactor, _statData.jumpCooldown);
                return;
            }
        }

        if (!_statData.enableJump && cliffAhead)
        {
            _monster.Rigid.velocity = new Vector2(0f, _monster.Rigid.velocity.y);
            return;
        }

        // --- 기본 수평 이동 ---
        float chargeBoost = (_statData.isCharging && _monster.PlayerInRange(_statData.chargeRange)) ? _statData.rushMultiplier : 1f;
        float targetX = dir * _monster.MonsterStats.MoveSpeed * chargeBoost;
        _monster.Rigid.velocity = new Vector2(targetX, _monster.Rigid.velocity.y);
    }

    public void Exit()
    {
        if (jumper.IsGrounded() && !jumper.IsMidJump)
            _monster.Rigid.velocity = Vector2.zero;
    }

    private void Orient(int dir)
    {
        Vector3 s = _monster.transform.localScale;
        s.x = Mathf.Abs(s.x) * dir;
        _monster.transform.localScale = s;
    }

    private void AttemptUnstuckSafer(int dir)
    {
        Debug.Log($"[{_monster.name}] Stuck! Attempting to un-stick.");
        if (_statData.enableJump && jumper.ReadyToJump())
        {
            jumper.PerformJump(dir, _statData.jumpForce * 0.1f, _statData.jumpForce, _statData.jumpHorizontalFactor, _statData.jumpCooldown);
        }
        else
        {
            _monster.Rigid.velocity = new Vector2(-Mathf.Sign(_monster.transform.localScale.x) * 1.5f, _monster.Rigid.velocity.y + 0.5f);
        }
        stuckTime = 0f;
    }

    private bool IsCliffAhead(int dir)
    {
        Vector2 origin = _monster.GroundSensor.position + (Vector3.right * dir * 0.3f);
        return !Physics2D.Raycast(origin, Vector2.down, 0.5f, _statData.groundMask);
    }
}