using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace SHL
{
    public class active : MonoBehaviour
    {
        public GameObject trigger;
        bool swich = true; // ����ġ ���¸� ��Ÿ���� ����  

        private void Awake()
        {
           
            
        }

        // Update is called once per frame
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.F) && swich)
            {
                Debug.Log("FŰ�� ���Ƚ��ϴ�.");
                StartCoroutine(TriggerOn()); // TriggerOn �ڷ�ƾ�� ȣ���Ͽ� Ʈ���Ÿ� Ȱ��ȭ�մϴ�.

                // ���⿡ FŰ�� ������ �� ������ �ڵ带 �߰��ϼ���.

            }
        }

        IEnumerator TriggerOn()
        {
            swich = false; // ����ġ�� ���� �ߺ� ���� ����
            trigger.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            trigger.SetActive(false);
            swich = true; // ����ġ�� �ٽ� �Ѽ� ���� ������ ���

        }
    }
}
