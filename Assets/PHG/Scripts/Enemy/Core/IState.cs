using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PHG
{
    public interface IState
    {
        void Enter(); // ���� ���� 1ȸ
        void Tick(); // �� ������(�Ǵ� Fixed) ȣ��
        void Exit(); // ���� ���� 1ȸ

    }
}