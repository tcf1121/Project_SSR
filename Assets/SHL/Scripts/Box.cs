using UnityEngine;
namespace SHL
{
    public class Box : MonoBehaviour
    {
        bool isOpen = false; //���ڰ� �����ִ��� ����
        private void Start()
        {
            Debug.Log(isOpen);
            // �ʱ�ȭ �۾��� �ʿ��ϴٸ� ���⿡ �ۼ�
            // ��: ���ڰ� �����ִ� ���·� ����
            isOpen = false;
        }
        public void BoxOpen()
        {
            if (!isOpen)
            {

                // Logic to open the box
                //�ڽ��� ���鼭 ��ȭ�� ����
                // ����: PlayerInventory���� ��ȭ ����
                // PlayerInventory.Instance.UseCurrency(10); // ���÷� 10�� ��ȭ�� ����Ѵٰ� ����
                Debug.Log("Box opened!"); //���ڿ��� ��� �������� ������
                isOpen = true; //���ڰ� �������� ǥ�� Ʈ���϶��� ���̻� ���ڸ� �� �� ����   
                //�����¾ִϸ��̼�
            }

        }

    }
}

