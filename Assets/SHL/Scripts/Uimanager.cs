using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SHL
{
    public class Uimanager : MonoBehaviour
    {
       public enum InfoType { Hp ,Exp , Coin , Time, MonsterHp ,TeleporterTimer,MaxUiBossHp,GuideMessage}
        public InfoType type;
        float timer ;
        Text mytext;
        Slider myslider;

        private void Awake()
        {
            mytext = GetComponent<Text>();
            myslider = GetComponent<Slider>();

        }

        private void LateUpdate()
        {
            switch(type)
            {
                case InfoType.Hp:
                    /*mytext.text = "HP : " + PlayerManager.instance.playerHp;
                     �÷��̾��� Hp ����*/
                    //float maxHp =
                    //float curHp = 
                    //myslider.value = curhp/ maxHp;
                    break;
                case InfoType.Exp:
                    /*mytext.text = "Exp : " + PlayerManager.instance.playerExp;
                     �÷��̾��� Exp ����*/
                    //float curExp =
                    //float maxExp = 
                    //myslider.value = curExp / maxExp;
                    break;
                case InfoType.Coin:
                    /*mytext.text = "Coin : " + PlayerManager.instance.playerCoin;
                     �÷��̾��� Coin ������*/
                    //mytext.text = string.Format("{0:F0}",�÷��̾� ���� ����);
                    break;
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
                    //��ȹ�� ����.
                    break;
            }
        }

    }
}