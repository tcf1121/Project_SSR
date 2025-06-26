using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SCR
{
    public class AlwaysOnUI : MonoBehaviour
    {
        private Player _player;

        [Header("Coin")]
        [SerializeField] private TMP_Text _coinText;

        [Header("Time")]
        [SerializeField] private TMP_Text _hourText;
        [SerializeField] private TMP_Text _minuteText;
        [SerializeField] private TMP_Text _secondText;
        [SerializeField] private TMP_Text _dangerText;

        [Header("State")]
        [SerializeField] private Image _fillHp;
        [SerializeField] private TMP_Text _maxHpText;
        [SerializeField] private TMP_Text _curHpText;
        [SerializeField] private Image _fillExp;
        [SerializeField] private TMP_Text _curExpText;
        [SerializeField] private TMP_Text _expPercentText;
        [SerializeField] private TMP_Text _levelText;

        [Header("Skill")]
        [SerializeField] private List<Image> _skillImage;
        [SerializeField] private List<TMP_Text> _coolTimeText;
        [SerializeField] private List<Image> _fillCoolTime;
        [SerializeField] private List<GameObject> _coolTimeObj;



        public void CoolTime(int index, bool IsCool)
        {
            _coolTimeObj[index].SetActive(IsCool);
        }

        public void SetCool(int index, float maxCoolTime, float curCoolTime)
        {
            _coolTimeText[index].text = curCoolTime.ToString("F1");
            _fillCoolTime[index].fillAmount = curCoolTime / maxCoolTime;
        }

        public void SetHp(int currentHp, int maxHp)
        {
            _curHpText.text = currentHp.ToString();
            _maxHpText.text = maxHp.ToString();
            _fillHp.fillAmount = (float)currentHp / maxHp;
        }

        public void SetExp(int currentExp, int maxExp)
        {
            float expPercent = (float)currentExp / maxExp;
            _curExpText.text = currentExp.ToString();
            _expPercentText.text = expPercent.ToString();
            _fillExp.fillAmount = expPercent;
        }

        public void SetLevel(int level)
        {
            _levelText.text = level.ToString();
        }

        public void SetCoin(int coin)
        {
            _coinText.text = coin.ToString();
        }

        public void SetTime(int hour, int minute, int second)
        {
            _hourText.text = hour.ToString();
            _minuteText.text = minute.ToString();
            _secondText.text = second.ToString();
        }

        public void SetDanger(int danger)
        {
            _dangerText.text = danger.ToString();
        }

        public void LinkedPlayer(Player player)
        {
            _player = player;
        }
    }

}
