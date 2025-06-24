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
            Time.timeScale = 0f; // ���� �ð��� 0���� ����� �Ͻ�����
            isPaused = true;
        }

        public void ResumeGame()
        {
            pauseMenuPanel.SetActive(false); // �޴� �г� ��Ȱ��ȭ
            Time.timeScale = 1f; // ���� �ð��� �������
            isPaused = false;
        }

        public void GoToLobby()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("�κ� �� �̸��� �־����");
        }

        public void SetVolume(float volume)
        {
            AudioListener.volume = volume;
        }
    }
}


