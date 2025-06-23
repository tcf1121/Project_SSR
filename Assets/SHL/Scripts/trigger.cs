using UnityEngine;

namespace SHL
{



    public class trigger : MonoBehaviour
    {


        public void OnTriggerEnter2D(Collider2D collision)
        {

            if (collision.gameObject.CompareTag("Box")) // 충돌한 오브젝트가 "Box" 태그를 가지고 있는지 확인
            {
                Debug.Log("상자발견");

                collision.gameObject.GetComponent<Box>().BoxOpen();

            }
        }
    }
}
