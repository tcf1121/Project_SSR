using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Utill
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private KeyCode pauseMenuKey = KeyCode.Escape;
        public GameObject pauseMenuPanel;
        public Slider soundSlider;
        
        private bool isPaused = false;

        private void Start()
        {
            pauseMenuPanel.SetActive(false);

            soundSlider.value = AudioListener.volume;
            soundSlider.onValueChanged.AddListener(SetVolume);
        }
        void Update()
        {
            if (Input.GetKeyDown(pauseMenuKey))
            {
                if (isPaused)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }
        }
        public void PauseGame()
        {
            pauseMenuPanel.SetActive(true);
            Time.timeScale = 0f; // 게임 시간을 0으로 만들어 일시정지
            isPaused = true;
        }

        public void ResumeGame()
        {
            pauseMenuPanel.SetActive(false); // 메뉴 패널 비활성화
            Time.timeScale = 1f; // 게임 시간을 원래대로
            isPaused = false;
        }

        public void GoToLobby()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("로비 씬 이름을 넣어야함");
        }

        public void SetVolume(float volume)
        {
            AudioListener.volume = volume;
        }
    }
}


