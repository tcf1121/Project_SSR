using UnityEngine;

namespace SHL
{



    public class trigger : MonoBehaviour
    {


        public void OnTriggerEnter2D(Collider2D collision)
        {

            if (collision.gameObject.CompareTag("Box")) // �浹�� ������Ʈ�� "Box" �±׸� ������ �ִ��� Ȯ��
            {
                Debug.Log("���ڹ߰�");

                collision.gameObject.GetComponent<Box>().BoxOpen();

            }
        }
    }
}
