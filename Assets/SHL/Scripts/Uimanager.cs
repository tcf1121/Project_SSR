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
                     플레이어의 Hp 연결*/
                    //float maxHp =
                    //float curHp = 
                    //myslider.value = curhp/ maxHp;
                    break;
                case InfoType.Exp:
                    /*mytext.text = "Exp : " + PlayerManager.instance.playerExp;
                     플레이어의 Exp 연결*/
                    //float curExp =
                    //float maxExp = 
                    //myslider.value = curExp / maxExp;
                    break;
                case InfoType.Coin:
                    /*mytext.text = "Coin : " + PlayerManager.instance.playerCoin;
                     플레이어의 Coin 소유량*/
                    //mytext.text = string.Format("{0:F0}",플레이어 코인 연결);
                    break;
                case InfoType.Time:
                    /*mytext.text = "Time : " + PlayerManager.instance.gameTime.ToString("F2");
                     게임의 절대값시간 흐른시간 난이도 조절부분 확정되지 않음으로 예비.*/
                    break;
                case InfoType.MonsterHp:
                    /*myslider.value = MonsterManager.instance.monsterHp / MonsterManager.instance.maxMonsterHp;
                     몬스터의 Hp 연결 /보스몬스터 포함 몬스터 상단부 작은 Hp 바를 뜻함
                    추가정보로 플레이어에게 공격을 받을시 활성화됨*/
                    //float m_maxHp =
                    //float m_curHp = 
                    //myslider.value = m_curhp/ m_maxHp;
                    break;
                case InfoType.TeleporterTimer:
                    /*mytext.text = "Teleporter Timer : " + TeleporterManager.instance.teleporterTimer.ToString("F2");
                     보스트리거인 텔레포터의 발동시 나오는 시간 타이머*/
                    
                    timer +=  Time.deltaTime;
                    
                   mytext.text = string.Format($"{timer:F0} : {90} seconds");
                    if(timer >= 90)
                    {
                        gameObject.SetActive(false); // 90초가 지나면 UI 비활성화
                    }

                    break;
                case InfoType.MaxUiBossHp:
                    /*myslider.value = BossManager.instance.bossHp / BossManager.instance.maxBossHp;
                     메인 상단부에 보스 등장시 활성화되는 보스Hp바*/
                    //float bm_maxHp =
                    //float bm_curHp = 
                    //myslider.value = bm_curhp/ bm_maxHp;
                    break;

                case InfoType.GuideMessage:
                    //기획후 조정.
                    break;
            }
        }

    }
}