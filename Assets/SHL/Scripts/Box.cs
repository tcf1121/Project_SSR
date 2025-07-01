using UnityEngine;
using LHE;
using Utill;
namespace SHL
{
    public class Box : MonoBehaviour
    {

        private LHE.PlayerStats _playerStats; // �÷��̾� ���� ��ũ��Ʈ�� �����ϱ� ���� ����
        Animator animator; // �ִϸ����� ������Ʈ�� �����ϱ� ���� ����
        bool isOpen = false; //���ڰ� �����ִ��� ����
        [SerializeField] float BoxOpenMoney = 25f; // ���ڸ� �� �� ����� ��ȭ�� ��
        //[SerializeField] GameObject[] Armitem;
        //[SerializeField] GameObject[] Headitem;
        //[SerializeField] GameObject[] Bodyitem;
        //[SerializeField] GameObject[] Legitem;
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
                   Itemmanager.instance.Chance(0.1f,0.2f,0.3f,0.4f);  //���� �Ӹ� ���� �� �ٸ� ��.

                isOpen = true; //���ڰ� �������� ǥ�� Ʈ���϶��� ���̻� ���ڸ� �� �� ����   
             
            }

        }
        #endregion
        
    }
}

