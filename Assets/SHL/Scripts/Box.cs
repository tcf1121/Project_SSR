using UnityEngine;
using LHE;
namespace SHL
{
    public class Box : MonoBehaviour
    {

        private LHE.PlayerStats _playerStats; // �÷��̾� ���� ��ũ��Ʈ�� �����ϱ� ���� ����
        Animator animator; // �ִϸ����� ������Ʈ�� �����ϱ� ���� ����
        bool isOpen = false; //���ڰ� �����ִ��� ����
        [SerializeField] float BoxOpenMoney = 25f; // ���ڸ� �� �� ����� ��ȭ�� ��
        [SerializeField] GameObject[] Armitem;
        [SerializeField] GameObject[] Headitem;
        [SerializeField] GameObject[] Bodyitem;
        [SerializeField] GameObject[] Legitem;
        private void Start()
        {
            _playerStats = GetComponent<LHE.PlayerStats>(); // �÷��̾� ���� ��ũ��Ʈ�� ������
            animator = GetComponent<Animator>(); // �ִϸ����� ������Ʈ�� ������
            // �ʱ�ȭ �۾��� �ʿ��ϴٸ� ���⿡ �ۼ�
            // ��: ���ڰ� �����ִ� ���·� ����
            isOpen = false;
        }
        /// <summary>
        /// ���ڸ� ���� �޼���
        /// </summary>
        #region 
        public void BoxOpen()
        {
            if (!isOpen)
            {
                //�����¾ִϸ��̼�
                animator.SetBool("IsOpen", true); // �ִϸ������� IsOpen �Ķ���͸� true�� �����Ͽ� ���� ���� �ִϸ��̼��� ���
                // Logic to open the box
                //�ڽ��� ���鼭 ��ȭ�� ����
                //_playerStats.SpendMoney((int)BoxOpenMoney); // ���÷� 10�� ��ȭ�� ����Ѵٰ� ����
                // ����: PlayerInventory���� ��ȭ ����
                // PlayerInventory.Instance.UseCurrency(10); // ���÷� 10�� ��ȭ�� ����Ѵٰ� ����
                //���ڰ� �������� ��� �䱸ġ�� ���
                BoxOpenMoney *= 1.2f; // ���ڸ� �� ������ �䱸�Ǵ� ��ȭ�� ���� ������Ŵ
                //20% �����ϸ� �Ҽ��� ���� ����
                //Debug.Log("Box opened!"); //���ڿ��� ��� �������� ������
                // ������ ���� ����
                // ��� ������ Ȯ�� �Ӹ������� 10% ���� 20% �� 30% �ٸ� 40%
                    Chance(0.3f,0.1f,0.2f,0.4f);

                isOpen = true; //���ڰ� �������� ǥ�� Ʈ���϶��� ���̻� ���ڸ� �� �� ����   
             
            }

        }
        #endregion
        /// <summary>
        /// 
        /// </summary>
        
        /// <param name="chance1"></param>�� Ȯ���� �������
        /// <param name="chance2"></param> 
        /// <param name="chance3"></param> 
        /// <param name="chance4"></param>
        void Chance(float chance1, float chance2, float chance3, float chance4)
        {
            float Randomnumber = Random.Range(0f, 1f); // 0���� 1 ������ ���� ���ڸ� ����
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

