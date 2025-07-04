using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace SHL
{
    public class active : MonoBehaviour
    {
        public GameObject trigger;
        bool swich = true; // 스위치 상태를 나타내는 변수  

        private void Awake()
        {
           
            
        }

        // Update is called once per frame
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.F) && swich)
            {
                Debug.Log("F키가 눌렸습니다.");
                StartCoroutine(TriggerOn()); // TriggerOn 코루틴을 호출하여 트리거를 활성화합니다.

                // 여기에 F키가 눌렸을 때 실행할 코드를 추가하세요.

            }
        }

        IEnumerator TriggerOn()
        {
            swich = false; // 스위치를 꺼서 중복 실행 방지
            trigger.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            trigger.SetActive(false);
            swich = true; // 스위치를 다시 켜서 다음 실행을 허용

        }
    }
}
