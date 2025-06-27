using UnityEngine;
using LHE;
namespace SHL
{
    public class Box : MonoBehaviour
    {

        private LHE.PlayerStats _playerStats; // 플레이어 보상 스크립트를 참조하기 위한 변수
        Animator animator; // 애니메이터 컴포넌트를 참조하기 위한 변수
        bool isOpen = false; //상자가 열려있는지 여부
        [SerializeField] float BoxOpenMoney = 25f; // 상자를 열 때 사용할 재화의 양
        [SerializeField] GameObject[] Armitem;
        [SerializeField] GameObject[] Headitem;
        [SerializeField] GameObject[] Bodyitem;
        [SerializeField] GameObject[] Legitem;
        private void Start()
        {
            _playerStats = GetComponent<LHE.PlayerStats>(); // 플레이어 보상 스크립트를 가져옴
            animator = GetComponent<Animator>(); // 애니메이터 컴포넌트를 가져옴
            // 초기화 작업이 필요하다면 여기에 작성
            // 예: 상자가 닫혀있는 상태로 시작
            isOpen = false;
        }
        /// <summary>
        /// 상자를 여는 메서드
        /// </summary>
        #region 
        public void BoxOpen()
        {
            if (!isOpen)
            {
                //열리는애니메이션
                animator.SetBool("IsOpen", true); // 애니메이터의 IsOpen 파라미터를 true로 설정하여 상자 열기 애니메이션을 재생
                // Logic to open the box
                //박스를 열면서 재화가 사용됨
                //_playerStats.SpendMoney((int)BoxOpenMoney); // 예시로 10의 재화를 사용한다고 가정
                // 예시: PlayerInventory에서 재화 감소
                // PlayerInventory.Instance.UseCurrency(10); // 예시로 10의 재화를 사용한다고 가정
                //상자가 열렸을때 골드 요구치량 상승
                BoxOpenMoney *= 1.2f; // 상자를 열 때마다 요구되는 재화의 양을 증가시킴
                //20% 증가하며 소수점 이하 절사
                //Debug.Log("Box opened!"); //상자에서 어떠한 아이템이 등장함
                // 아이템 등장 로직
                // 드랍 아이템 확률 머리아이템 10% 몸통 20% 팔 30% 다리 40%
                    Chance(0.3f,0.1f,0.2f,0.4f);

                isOpen = true; //상자가 열렸음을 표시 트루일때는 더이상 상자를 열 수 없음   
             
            }

        }
        #endregion
        /// <summary>
        /// 
        /// </summary>
        
        /// <param name="chance1"></param>각 확률을 적어야함
        /// <param name="chance2"></param> 
        /// <param name="chance3"></param> 
        /// <param name="chance4"></param>
        void Chance(float chance1, float chance2, float chance3, float chance4)
        {
            float Randomnumber = Random.Range(0f, 1f); // 0부터 1 사이의 랜덤 숫자를 생성
            if (Randomnumber < chance1)
            {
                //Debug.Log("Armitem");
                int randomIndex = Random.Range(0, Armitem.Length);
                Instantiate(Armitem[randomIndex], transform.position, Quaternion.identity);
            }
            else if (Randomnumber < chance2+chance1)
            {
                //Debug.Log("Headitem");
                int randomIndex = Random.Range(0, Headitem.Length);
                Instantiate(Headitem[randomIndex], transform.position, Quaternion.identity);
            }
            else if (Randomnumber < chance3+chance2+chance1)
            {
                //Debug.Log("Bodyitem");
                int randomIndex = Random.Range(0, Bodyitem.Length);
                Instantiate(Bodyitem[randomIndex], transform.position, Quaternion.identity);
            }
            else if (Randomnumber < chance4+chance3+chance2+chance1)
            {
                //Debug.Log("Legitem");
                int randomIndex = Random.Range(0, Legitem.Length);
                Instantiate(Legitem[randomIndex], transform.position, Quaternion.identity);
            }


        }
    }
}

