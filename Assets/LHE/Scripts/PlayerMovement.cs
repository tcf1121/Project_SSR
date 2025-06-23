using UnityEngine;

[System.Serializable]
public class PlayerMovementConfig
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float acceleration = 50f;
    public float deceleration = 50f;

    [Header("Jump")]
    public float jumpForce = 15f;
    public float jumpCutMultiplier = 0.5f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public float coyoteTime = 0.2f;
    public float jumpBufferTime = 0.2f;

    [Header("Dash")]
    public float dashForce = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Wall")]
    public float wallSlideSpeed = 2f;
    public float wallJumpForce = 12f;
    public Vector2 wallJumpDirection = new Vector2(1f, 1.5f);
    public float wallJumpTime = 0.2f;

    [Header("Crouch")]
    public float crouchSpeedMultiplier = 0.5f;
    public float crouchColliderHeight = 0.5f;

    [Header("Climb")]
    public float climbSpeed = 5f;
}