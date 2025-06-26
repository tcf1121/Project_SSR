using UnityEngine;

namespace LHE
{
    public class NormalAttack : MonoBehaviour
    {
        private PlayerStats playerStats;

        // 변수
        // 공격 지속시간
        // 공격 쿨타임
        // 적의 레이어

        // 공격 범위를 가질 콜라이더
        // 그 콜라이더의 위치

        // 디버그

        // 컴포넌트 가져오기

        // 업데이트에서 쿨타임 처리

        // x키 입력처리

        // 공격 로직
        // 공격 가능 여부
        // 공격 실행

        // 공격 코루틴 (공격실행에 들어감)
        // ㄴ 공격 시작 , 대기 , 공격 종료

        // 공격 시작
        // 공격중 상태, 쿨타임실행, 공격 콜라이더 활성화, 애니메이션

        // 공격종려
        // 위사항 반대

        // 충돌 처리
        // 공격중이 아니면 무시
        // 적레이어인지 확인
        // 중복 방지 구현 필요
        // 데미지 처리
        // 
        // 데미지 처리는 다른분이 짠 몬스터 스크립트 확인하여 연결할것



        void Awake()
        {
            playerStats = GetComponent<PlayerStats>();
        }


        void Update()
        {

        }
    }
}
