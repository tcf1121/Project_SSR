using UnityEngine;

public class PatrolState : IState
{
    private Monster _monster;
    private readonly MonsterStatEntry _statData;

    private int dir = 1;
    private const float floorCheckDist = 0.4f; // 사용되지 않을 수 있지만 남겨둠
    private const float wallCheckDist = 0.2f;

    // 벽 끼임 반복 감지 및 탈출을 위한 필드 (낭떠러지 -> 벽 끼임으로 변경)
    private int stuckDetectionCount = 0; // cliffDetectionCount -> stuckDetectionCount
    private float stuckDetectionTimer = 0f; // cliffDetectionTimer -> stuckDetectionTimer
    private const float STUCK_DETECTION_WINDOW = 1.0f; // 1초 동안 감지 횟수 누적
    private const int STUCK_DETECTION_THRESHOLD = 3; // 3번 이상 감지 시 탈출 시도
    private const float UNSTUCK_Y_OFFSET = 1.0f; // Y축으로 올릴 값 (갇혔을 때 살짝 위로 이동)
    private const float UNSTUCK_X_OFFSET = 0.5f; // X축으로 이동할 값 (벽에 끼었을 때 옆으로 이동)


    public PatrolState(Monster monster)
    {
        _monster = monster;
        _statData = monster.Brain.StatData;
    }

    public void Enter()
    {
        if (_monster.Brain.StatData.hasIdleAnim)
            _monster.PlayAnim(AnimNames.Walk);
        _monster.Rigid.velocity = Vector2.zero;
        dir = _monster.Transfrom.localScale.x >= 0f ? 1 : -1;

        // 상태 진입 시 갇힘 관련 변수 초기화
        stuckDetectionCount = 0;
        stuckDetectionTimer = 0f;
    }

    public void Tick()
    {
        if (_statData == null) return;

        _monster.Rigid.velocity = new Vector2(dir * _monster.MonsterStats.MoveSpeed, _monster.Rigid.velocity.y);

        // GroundSensor는 낭떠러지 감지에 주로 사용되므로, 벽 끼임 감지에서는 WallSensor를 활용
        // Vector2 groundCheckPos = _monster.GroundSensor.position + Vector3.right * dir * 0.3f + Vector3.down * 0.1f; // 더 이상 사용 안 함
        // float groundCheckRadius = 0.1f; // 더 이상 사용 안 함

        bool noFloor = !Physics2D.OverlapCircle(_monster.GroundSensor.position, 0.1f, _statData.groundMask); // 기존 낭떠러지 감지 (방향 전환용)
        bool hitWall = Physics2D.Raycast(_monster.WallSensor.position, Vector2.right * dir, wallCheckDist, _statData.wallMask);

        // --- 벽 끼임 감지 횟수 누적 및 타이머 처리 (수정된 로직) ---
        // 벽에 부딪혔고 (hitWall), 몬스터가 움직이지 않는 상태 (또는 거의 움직이지 않는 상태)일 때를 '끼임'으로 판단
        // Rigidbody.velocity.magnitude가 0에 가까운지 확인하여 벽에 박혀 멈췄는지 감지
        if (hitWall && _monster.Rigid.velocity.magnitude < 0.1f) // 벽에 부딪히고 거의 움직이지 않을 때
        {
            stuckDetectionCount++;
            Debug.DrawRay(_monster.WallSensor.position, Vector2.right * dir * wallCheckDist, Color.yellow); // 디버그 시각화
        }
        else
        {
            // 벽에 부딪히지 않았거나, 움직임이 있다면 카운트와 타이머 초기화
            stuckDetectionCount = 0;
            stuckDetectionTimer = 0f;
        }

        stuckDetectionTimer += Time.deltaTime;

        // 일정 시간 동안 벽 끼임 감지가 지속되지 않았다면 카운트 초기화
        if (stuckDetectionTimer >= STUCK_DETECTION_WINDOW)
        {
            if (stuckDetectionCount < STUCK_DETECTION_THRESHOLD) // 임계값 미만이면 초기화
            {
                stuckDetectionCount = 0;
            }
            stuckDetectionTimer = 0f; // 타이머는 항상 리셋
        }


        // 벽 끼임 감지 횟수가 임계값을 초과하면 강제 탈출 (Y축 또는 X축 직접 이동)
        if (stuckDetectionCount >= STUCK_DETECTION_THRESHOLD)
        {
            Debug.Log($"[{_monster.name}] PatrolState: 반복적인 벽 끼임 감지! 탈출 시도.");

            // 갇힘 탈출 시도: Y축으로 살짝 올리거나, X축으로 반대 방향으로 이동 시도
            // 상황에 따라 둘 중 하나 또는 둘 다 사용
            _monster.transform.position = new Vector3(
                _monster.transform.position.x - dir * UNSTUCK_X_OFFSET, // 현재 이동 방향의 반대 방향으로 X축 이동
                _monster.transform.position.y + UNSTUCK_Y_OFFSET,       // Y축으로 살짝 올림
                _monster.transform.position.z);
            _monster.Rigid.velocity = Vector2.zero; // 이동 후 속도 초기화 (안정화)

            // 이동 후 방향 전환 (끼인 방향에서 벗어나기 위해)
            dir *= -1;
            Vector3 scale = _monster.Transfrom.localScale;
            scale.x = Mathf.Abs(scale.x) * dir;
            _monster.Transfrom.localScale = scale;

            stuckDetectionCount = 0;
            stuckDetectionTimer = 0f;
            return; // 갇힘 방지 로직이 실행되었으므로 현재 틱은 여기서 종료
        }
        // --- 벽 끼임 감지 로직 수정 끝 ---


        // 기존 방향 전환 로직 (낭떠러지 또는 벽 감지 시) - 이 부분은 위 갇힘 방지 로직과 별개로 작동
        // 이제 'noFloor'는 순전히 낭떠러지에서 방향을 바꾸는 용도로만 사용됩니다.
        if (noFloor || hitWall) // hitWall은 여기서도 방향 전환 용도로 사용
        {
            dir *= -1;
            Vector3 scale = _monster.Transfrom.localScale;
            scale.x = Mathf.Abs(scale.x) * dir;
            _monster.Transfrom.localScale = scale;
        }

        // 0순위: 비행 원거리형 유닛 전용 추적 (FloatChase)
        if (_statData.isRanged && _statData.isFlying && _monster.PlayerInRange(_statData.readyRange) && _monster.PlayerInRange(_statData.patrolRange))
        {
            _monster.ChangeState(StateID.FloatChase);
            return;
        }

        // 1순위: 추적 조건 — 모든 유닛 공통
        else if (_monster.PlayerInRange(_statData.patrolRange))
        {
            _monster.ChangeState(StateID.Chase);
            return;
        }

        // 2순위: 공격 조건
        else if (_monster.PlayerInRange(_statData.attackRange))
        {
            _monster.ChangeState(StateID.Attack);
            return;
        }

        // 3순위: readyRange는 상태 전이 없음 (조준만)
        else if (_statData.isRanged &&
                 _monster.Brain.StateMachine.CurrentStateID != StateID.Chase &&
                 _monster.PlayerInRange(_statData.readyRange))
        {
            FacePlayer();
            _monster.Rigid.velocity = Vector2.zero;
            _monster.ChangeState(StateID.AimReady);
            return;
        }

        // 사다리 AI (순찰 중에도 플레이어가 범위 내에 있으면 사다리 시도)
        if (_statData.enableLadderClimb && _monster.PlayerInRange(_statData.patrolRange))
        {
            if (_monster.Target != null)
                _monster.Brain.Climber?.TryFindAndClimb(dir);
        }
    }

    public void Exit()
    {
        _monster.Rigid.velocity = Vector2.zero;
        // 상태 종료 시 갇힘 관련 변수 초기화
        stuckDetectionCount = 0;
        stuckDetectionTimer = 0f;
    }

    private void FacePlayer()
    {
        if (_monster.Target == null) return;

        int sign = _monster.Target.position.x > _monster.transform.position.x ? 1 : -1;
        Vector3 s = _monster.transform.localScale;
        s.x = Mathf.Abs(s.x) * sign;
        _monster.transform.localScale = s;
    }
}