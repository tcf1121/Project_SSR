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
        private LHE.PlayerStats _playerStats; // 플레이어 스탯을 참조하기 위한 변수
        public enum InfoType { Hp ,Exp , Coin , Time, MonsterHp ,TeleporterTimer,MaxUiBossHp,GuideMessage,StartMessage}
        public InfoType type;
        bool messageCorutine = true; // 가이드 메시지 코루틴
        bool isStartMessage = true; // 시작 메시지 코루틴
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
                //     플레이어의 Hp 연결*/

                //    float maxHp = _playerStats.FinalMaximumHp;
                //    float curHp = _playerStats.CurrentHp;
                //    myslider.value = curHp/ maxHp;
                //    break;
                //case InfoType.Exp:
                //    /*mytext.text = "Exp : " + PlayerManager.instance.playerExp;
                //     플레이어의 Exp 연결*/

                //    float curExp =_playerStats.CurrentExp;
                //    //float maxExp = _playerStats.ReqExp;
                //    //myslider.value = curExp / maxExp;
                //    break;
                //case InfoType.Coin:
                    
                //    mytext.text = string.Format("{0:F0}", _playerStats.Money);
                //    //플레이어의 Coin 소유량
                //    //mytext.text = string.Format("{0:F0}",플레이어 코인 연결);
                //    break;
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
                    if(messageCorutine)
                    {
                        StartCoroutine(guidemessage(guide1,guide2,10,5));
                    }
                    //기획후 조정.
                    break;
                case InfoType.StartMessage:
                    if(isStartMessage)
                    {
                        // 게임 시작 메시지 표시
                        // 0은 스테이지 번호를 받아옴 
                        //1은 스테이지   이름을 받아옴
                        StartCoroutine(_startmessage());
                        // 시작 메시지는 한 번만 표시
                       
                    }
                    //mytext.text = string.Format("스테이지 {0} {1}","1","스테이지 이름");
                    //기획후 조정.
                    break;
            }
            
        }
        string guide1 = "텔레포트 장치를 찾으세요";
        string guide2 = "텔레포트 시부렁시부렁 뭐시기저시기";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="one"></param>메세지1
        /// <param name="two"></param>메세지2
        /// <param name="waittime"></param>대기시간1
        /// <param name="waittime2"></param> 대기시간2
        
        /// <returns></returns>
        IEnumerator guidemessage(string one,string two,int waittime,int waittime2)
        {             //기획후 조정.
            messageCorutine = false; // 코루틴이 실행 중임을 표시
            yield return new WaitForSeconds(waittime); // 10초 대기
            mytext.text = string.Format(one);
            yield return new WaitForSeconds(waittime2);
            mytext.text = string.Format(two);
            yield return new WaitForSeconds(waittime2);
            messageCorutine = true; // 코루틴이 끝났음을 표시
              
        }
        IEnumerator _startmessage()
        {
            isStartMessage = false; // 시작 메시지 표시 후 재실행금지.
            yield return new WaitForSeconds(2); // 2초 대기
            mytext.text = string.Format("스테이지 {0} {1}", "1", "스테이지 이름");
            yield return new WaitForSeconds(4); // 4초 대기
            mytext.text = string.Format($"{guide1}");
            yield return new WaitForSeconds(4); // 4초 대기
            gameObject.SetActive(false); // 메시지 표시 후 UI 비활성화
        }
    }
}