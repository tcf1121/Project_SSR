using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler
{
    public Vector2 MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool DashPressed { get; private set; }
    public bool CrouchHeld { get; private set; }

    // Timers
    public float JumpBufferTimer { get; private set; }

    private float jumpBufferTime = 0.2f;

    public void Update()
    {
        // Update timers
        if (JumpBufferTimer > 0)
            JumpBufferTimer -= Time.deltaTime;

        // Reset frame-based inputs
        JumpPressed = false;
        DashPressed = false;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            JumpPressed = true;
            JumpBufferTimer = jumpBufferTime;
        }

        JumpHeld = context.ReadValue<float>() > 0;
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed)
            DashPressed = true;
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        CrouchHeld = context.ReadValue<float>() > 0;
    }
}