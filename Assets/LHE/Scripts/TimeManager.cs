using TMPro;
using UnityEngine;

namespace LHE
{
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }

        // �ð�
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
        /// �ð� ������Ʈ
        /// </summary>
        private void TimeUpdate()
        {
            TimeSeconds();
            TimeMinutes();
            TimeHours();
            MaximumTime();
        }

        /// <summary>
        /// �� ������Ʈ
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
        /// �� ������Ʈ
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
        /// �� ������Ʈ
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
        /// �ð� �ִ�ġ (���޽� �ڵ����� ����)
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
        /// �ð� ����
        /// </summary>
        public void TimeStop() { IsStop = true; }
        /// <summary>
        /// �ð����
        /// </summary>
        public void TimePaly() { IsStop = false; }
        /// <summary>
        /// �ð� ����
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