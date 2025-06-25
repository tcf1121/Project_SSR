using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PHG
{
    public interface IState
    {
        void Enter(); // 상태 진입 1회
        void Tick(); // 매 프레임(또는 Fixed) 호출
        void Exit(); // 상태 종료 1회

    }
}