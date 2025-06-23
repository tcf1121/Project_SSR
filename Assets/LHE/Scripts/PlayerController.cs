using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.WSA;

public class NewBehaviourScript : MonoBehaviour
{
    // 컨트롤 변수 (카운터 등)
    private float jumpBufferCounter;

    // 인풋 변수
    private Vector2 moveInput;
    private bool jumpInputDown;
    private bool jumpInput;
    private bool dashInput;

    // 중복 방지 변수
    private float jumpBufferTime = 0.2f;


    #region 인풋
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // 누른 즉시
            // jumpInputDown = true;
            jumpBufferCounter = jumpBufferTime;
        }
        
        // 눌려있는 상태 트루, 땔때 flase / 눌린 시간 비례 점프
        jumpInput = context.ReadValue<float>() > 0;
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            dashInput = true;
        }
    }

    #endregion
    // 점프 강도 조절 활용? // Interactions 사용
    //public void OnJump(InputAction.CallbackContext context)
    //{
    //    if (context.started)
    //    {
    //        // 버튼을 누르기 시작했을 때
    //        // 점프 준비 애니메이션 등
    //    }

    //    if (context.performed)
    //    {
    //        // 버튼을 완전히 눌렀을 때
    //        // 실제 점프 실행
    //    }

    //    if (context.canceled)
    //    {
    //        // 버튼을 뗐을 때
    //        // 가변 점프 높이 등
    //    }
    //}
}
