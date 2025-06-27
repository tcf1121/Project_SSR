using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace SCR
{
    public class PlayerPhysical : MonoBehaviour
    {
        private Player player;

        [Header("이동 설정")]
        [SerializeField] private float _acceleration = 50f;
        [SerializeField] private float _deceleration = 30f;
        [SerializeField] private float _airMoveSpeedMultiplier = 0.75f; // 공중 이동 속도 배율

        [Header("점프 설정")]
        [SerializeField] private float _jumpBufferTime = 0.2f;

        [Header("하단 점프 설정")]
        [SerializeField] private float _dropThroughTime = 0.3f;  // 하단점프 관통 지속 시간

        [Header("대쉬 설정")]
        [SerializeField] private float _dashForce = 5f;
        [SerializeField] private float _dashDuration = 0.2f;
        [SerializeField] private float _dashEndSpeedRatio = 0.2f;

        [Header("앉기 설정")]
        [SerializeField] private float _crouchSpeedMultiplier = 0.5f;  // 앉기 시 속도 배율

        [Header("벽 붙잡기 설정")]
        [SerializeField] private float _wallSlideSpeed = 1.2f; // 벽 슬라이드 속도
        [SerializeField] private float _wallCheckDistance = 0.6f; // 벽 감지 거리
        [SerializeField] private float _wallSlideDelayTime = 0.1f; // 벽잡기 활성화 지연 시간
        [SerializeField] private float _wallJumpInputBlockTime = 0.4f; // 벽점프 후 입력 차단 시간
        [SerializeField] private bool _isWallJumpInputBlocked = false; // 입력 차단 상태

        [Header("사다리/밧줄 설정")]
        [SerializeField] private float _climbSpeed = 3f; // 사다리 오르는 속도
        [SerializeField] private float _ladderActionDelay = 0.3f; // 사다리 재진입 딜레이

        [Header("환경 감지")]
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private GameObject _pickTrigger;
        [SerializeField] private Vector2 _groundCheckBoxSize = new Vector2(0.8f, 0.05f);
        [SerializeField] private LayerMask _groundLayerMask = 1 << 9; // 관통 불가능 (돌, 벽돌 등)
        [SerializeField] private LayerMask _platformLayer = 1 << 10; // 관통 가능 (나무 판자, 구름 등)
        [SerializeField] private LayerMask _allGroundLayers = (1 << 9) | (1 << 10); // 모든 바닥
        [SerializeField] private LayerMask _ladderLayer = 1 << 11;  // 사다리/밧줄 레이어

        public float Acceleration { get { return _acceleration; } }
        public float Deceleration { get { return _deceleration; } }
        public float AirMoveSpeedMultiplier { get { return _airMoveSpeedMultiplier; } }

        public float JumpBufferTime { get { return _jumpBufferTime; } }

        public float DropThroughTime { get { return _dropThroughTime; } }

        public float DashForce { get { return _dashForce; } }
        public float DashDuration { get { return _dashDuration; } }
        public float DashEndSpeedRatio { get { return _dashEndSpeedRatio; } }

        public float CrouchSpeedMultiplier { get { return _crouchSpeedMultiplier; } }

        public float WallSlideSpeed { get { return _wallSlideSpeed; } }
        public float WallCheckDistance { get { return _wallCheckDistance; } }
        public float WallSlideDelayTime { get { return _wallSlideDelayTime; } }
        public float WallJumpInputBlockTime { get { return _wallJumpInputBlockTime; } }
        public bool IsWallJumpInputBlocked { get { return _isWallJumpInputBlocked; } set { _isWallJumpInputBlocked = value; } }

        public float ClimbSpeed { get { return _climbSpeed; } }
        public float LadderActionDelay { get { return _ladderActionDelay; } }

        public Transform GroundCheck { get { return _groundCheck; } }
        public Vector2 GroundCheckBoxSize { get { return _groundCheckBoxSize; } }
        public LayerMask GroundLayerMask { get { return _groundLayerMask; } }
        public LayerMask PlatformLayer { get { return _platformLayer; } }
        public LayerMask AllGroundLayers { get { return _allGroundLayers; } }
        public LayerMask LadderLayer { get { return _ladderLayer; } }
        public GameObject PickTrigger { get { return _pickTrigger; } }

        private float _finalSpeed;
        public float FinalSpeed { get { return _finalSpeed; } }

        private float _finalJump;
        public float FinalJump { get { return _finalJump; } }

        void Awake()
        {
            player = GetComponent<Player>();
        }

        void Start()
        {
            SetSpeed();
            SetJump();
        }

        public void SetGroundCheck(Transform groundCheck)
        {
            _groundCheck = groundCheck;
        }


        public void SetSpeed()
        {
            _finalSpeed = 4f + player.PlayerStats.FinalSpeed * 0.2f;
        }

        public void SetJump()
        {
            _finalJump = 5f + player.PlayerStats.FinalJump * 0.2f;
        }

    }
}
