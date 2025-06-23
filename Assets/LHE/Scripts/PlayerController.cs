using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.WSA;

public class NewBehaviourScript : MonoBehaviour
{
    // ��Ʈ�� ���� (ī���� ��)
    private float jumpBufferCounter;

    // ��ǲ ����
    private Vector2 moveInput;
    private bool jumpInputDown;
    private bool jumpInput;
    private bool dashInput;

    // �ߺ� ���� ����
    private float jumpBufferTime = 0.2f;


    #region ��ǲ
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // ���� ���
            // jumpInputDown = true;
            jumpBufferCounter = jumpBufferTime;
        }
        
        // �����ִ� ���� Ʈ��, ���� flase / ���� �ð� ��� ����
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
    // ���� ���� ���� Ȱ��? // Interactions ���
    //public void OnJump(InputAction.CallbackContext context)
    //{
    //    if (context.started)
    //    {
    //        // ��ư�� ������ �������� ��
    //        // ���� �غ� �ִϸ��̼� ��
    //    }

    //    if (context.performed)
    //    {
    //        // ��ư�� ������ ������ ��
    //        // ���� ���� ����
    //    }

    //    if (context.canceled)
    //    {
    //        // ��ư�� ���� ��
    //        // ���� ���� ���� ��
    //    }
    //}
}
