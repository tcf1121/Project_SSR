using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LHE;
using System.Data.SqlTypes;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using UnityEngine.Events;

namespace SHL
{
    public class Uimanager : MonoBehaviour
    {
        private LHE.PlayerStats _playerStats; // �÷��̾� ������ �����ϱ� ���� ����
        public enum InfoType { Hp ,Exp , Coin , Time, MonsterHp ,TeleporterTimer,MaxUiBossHp,GuideMessage,StartMessage}
        public InfoType type;
        bool messageCorutine = true; // ���̵� �޽��� �ڷ�ƾ
        bool isStartMessage = true; // ���� �޽��� �ڷ�ƾ
        float timer ;
        Text mytext;
        Slider myslider;

        private void Awake()
        {
            mytext = GetComponent<Text>();
            myslider = GetComponent<Slider>();
            _playerStats =GetComponent<LHE.PlayerStats>();
        }

        private void LateUpdate()
        {
            switch(type)
            {
                //case InfoType.Hp:
                //    /*mytext.text = "HP : " + PlayerManager.instance.playerHp;
                //     �÷��̾��� Hp ����*/

                //    float maxHp = _playerStats.FinalMaximumHp;
                //    float curHp = _playerStats.CurrentHp;
                //    myslider.value = curHp/ maxHp;
                //    break;
                //case InfoType.Exp:
                //    /*mytext.text = "Exp : " + PlayerManager.instance.playerExp;
                //     �÷��̾��� Exp ����*/

                //    float curExp =_playerStats.CurrentExp;
                //    //float maxExp = _playerStats.ReqExp;
                //    //myslider.value = curExp / maxExp;
                //    break;
                //case InfoType.Coin:
                    
                //    mytext.text = string.Format("{0:F0}", _playerStats.Money);
                //    //�÷��̾��� Coin ������
                //    //mytext.text = string.Format("{0:F0}",�÷��̾� ���� ����);
                //    break;
                case InfoType.Time:
                    /*mytext.text = "Time : " + PlayerManager.instance.gameTime.ToString("F2");
                     ������ ���밪�ð� �帥�ð� ���̵� �����κ� Ȯ������ �������� ����.*/
                    break;
                case InfoType.MonsterHp:
                    /*myslider.value = MonsterManager.instance.monsterHp / MonsterManager.instance.maxMonsterHp;
                     ������ Hp ���� /�������� ���� ���� ��ܺ� ���� Hp �ٸ� ����
                    �߰������� �÷��̾�� ������ ������ Ȱ��ȭ��*/
                    //float m_maxHp =
                    //float m_curHp = 
                    //myslider.value = m_curhp/ m_maxHp;
                    break;
                case InfoType.TeleporterTimer:
                    /*mytext.text = "Teleporter Timer : " + TeleporterManager.instance.teleporterTimer.ToString("F2");
                     ����Ʈ������ �ڷ������� �ߵ��� ������ �ð� Ÿ�̸�*/
                    
                    timer +=  Time.deltaTime;
                    
                   mytext.text = string.Format($"{timer:F0} : {90} seconds");
                    if(timer >= 90)
                    {
                        gameObject.SetActive(false); // 90�ʰ� ������ UI ��Ȱ��ȭ
                    }

                    break;
                case InfoType.MaxUiBossHp:
                    /*myslider.value = BossManager.instance.bossHp / BossManager.instance.maxBossHp;
                     ���� ��ܺο� ���� ����� Ȱ��ȭ�Ǵ� ����Hp��*/
                    //float bm_maxHp =
                    //float bm_curHp = 
                    //myslider.value = bm_curhp/ bm_maxHp;
                    break;

                case InfoType.GuideMessage:
                    if(messageCorutine)
                    {
                        StartCoroutine(guidemessage(guide1,guide2,10,5));
                    }
                    //��ȹ�� ����.
                    break;
                case InfoType.StartMessage:
                    if(isStartMessage)
                    {
                        // ���� ���� �޽��� ǥ��
                        // 0�� �������� ��ȣ�� �޾ƿ� 
                        //1�� ��������   �̸��� �޾ƿ�
                        StartCoroutine(_startmessage());
                        // ���� �޽����� �� ���� ǥ��
                       
                    }
                    //mytext.text = string.Format("�������� {0} {1}","1","�������� �̸�");
                    //��ȹ�� ����.
                    break;
            }
            
        }
        string guide1 = "�ڷ���Ʈ ��ġ�� ã������";
        string guide2 = "�ڷ���Ʈ �úη��úη� ���ñ����ñ�";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="one"></param>�޼���1
        /// <param name="two"></param>�޼���2
        /// <param name="waittime"></param>���ð�1
        /// <param name="waittime2"></param> ���ð�2
        
        /// <returns></returns>
        IEnumerator guidemessage(string one,string two,int waittime,int waittime2)
        {             //��ȹ�� ����.
            messageCorutine = false; // �ڷ�ƾ�� ���� ������ ǥ��
            yield return new WaitForSeconds(waittime); // 10�� ���
            mytext.text = string.Format(one);
            yield return new WaitForSeconds(waittime2);
            mytext.text = string.Format(two);
            yield return new WaitForSeconds(waittime2);
            messageCorutine = true; // �ڷ�ƾ�� �������� ǥ��
              
        }
        IEnumerator _startmessage()
        {
            isStartMessage = false; // ���� �޽��� ǥ�� �� ��������.
            yield return new WaitForSeconds(2); // 2�� ���
            mytext.text = string.Format("�������� {0} {1}", "1", "�������� �̸�");
            yield return new WaitForSeconds(4); // 4�� ���
            mytext.text = string.Format($"{guide1}");
            yield return new WaitForSeconds(4); // 4�� ���
            gameObject.SetActive(false); // �޽��� ǥ�� �� UI ��Ȱ��ȭ
        }
    }
}