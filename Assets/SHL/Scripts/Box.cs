using UnityEngine;
namespace SHL
{
    public class Box : MonoBehaviour
    {
        bool isOpen = false; //상자가 열려있는지 여부
        private void Start()
        {
            Debug.Log(isOpen);
            // 초기화 작업이 필요하다면 여기에 작성
            // 예: 상자가 닫혀있는 상태로 시작
            isOpen = false;
        }
        public void BoxOpen()
        {
            if (!isOpen)
            {

                // Logic to open the box
                //박스를 열면서 재화가 사용됨
                // 예시: PlayerInventory에서 재화 감소
                // PlayerInventory.Instance.UseCurrency(10); // 예시로 10의 재화를 사용한다고 가정
                Debug.Log("Box opened!"); //상자에서 어떠한 아이템이 등장함
                isOpen = true; //상자가 열렸음을 표시 트루일때는 더이상 상자를 열 수 없음   
                //열리는애니메이션
            }

        }

    }
}

