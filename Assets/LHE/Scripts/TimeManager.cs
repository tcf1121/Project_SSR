using TMPro;
using UnityEngine;

namespace LHE
{
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }

        // 시간
        [SerializeField] int seconds = 0;
        [SerializeField] int minutes = 0;
        [SerializeField] int hours = 0;
        private float count = 0;

        // UI
        [SerializeField] private TextMeshProUGUI secondsText;
        [SerializeField] private TextMeshProUGUI minutesText;
        [SerializeField] private TextMeshProUGUI hoursText;

        private bool IsStop = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Update()
        {
            if (IsStop) return;

            TimeUpdate();
        }

        /// <summary>
        /// 시간 업데이트
        /// </summary>
        private void TimeUpdate()
        {
            TimeSeconds();
            TimeMinutes();
            TimeHours();
            MaximumTime();
        }

        /// <summary>
        /// 초 업데이트
        /// </summary>
        private void TimeSeconds()
        {
            count += Time.deltaTime;

            if (count >= 1)
            {
                count = 0;
                seconds++;
                secondsText.text = seconds.ToString("D2");
            }
        }

        /// <summary>
        /// 분 업데이트
        /// </summary>
        private void TimeMinutes()
        {
            if (seconds >= 60)
            {
                seconds = 0;
                minutes++;
                minutesText.text = minutes.ToString("D2");
            }
        }

        /// <summary>
        /// 시 업데이트
        /// </summary>
        private void TimeHours()
        {
            if (minutes >= 60)
            {
                minutes = 0;
                hours++;
                hoursText.text = hours.ToString("D2");
            }
        }

        /// <summary>
        /// 시간 최대치 (도달시 자동으로 정지)
        /// </summary>
        private void MaximumTime()
        {
            if (hours >= 99)
            {
                IsStop = true;
                seconds = 0;
                minutes = 0;
                hours = 0;
            }
        }

        /// <summary>
        /// 시간 정지
        /// </summary>
        public void TimeStop() { IsStop = true; }
        /// <summary>
        /// 시간재생
        /// </summary>
        public void TimePaly() { IsStop = false; }
        /// <summary>
        /// 시간 리셋
        /// </summary>
        public void TimeReset()
        {
            count = 0;
            seconds = 0;
            minutes = 0;
            hours = 0;
        }
    }
}