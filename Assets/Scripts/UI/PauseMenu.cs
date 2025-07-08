using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class PauseMenu : MonoBehaviour
{
    [SerializeField] private KeyCode pauseMenuKey = KeyCode.Escape;

    [Header("UI")]
    public GameObject warningPopup; // 경고 팝업 UI
    public GameObject pauseMenuPanel;

    public Slider soundSlider;
    public Slider bgmSlider; // 배경음 슬라이더
    public Slider sfxSlider; // 효과음 슬라이더
    [Header("해상도")]
    public TMPro.TMP_Dropdown resolutionDropdown; // 해상도 드롭다운
    public Toggle fullscreenToggle; // 전체화면 토글

    [Header("오디오 믹서")]
    public AudioMixer mainAudioMixer;

    private bool isPaused = false;
    private bool isGameScene = false; // 게임 진행중이면 true
    private Resolution[] resolutions; // 사용 가능한 해상도 목록

    private void Awake()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> resolutionOptions = new List<string>();
        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            resolutionOptions.Add(option);

            // 현재 해상도를 드롭다운의 기본값으로 설정
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        resolutionDropdown.AddOptions(resolutionOptions);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        fullscreenToggle.isOn = Screen.fullScreen; // 전체화면 토글 상태 설정
    }

    private void Start()
    {
        pauseMenuPanel.SetActive(false);
        warningPopup.SetActive(false);

        isGameScene = SceneManager.GetActiveScene().name != "타이틀 씬 이름"; // TODO: 타이틀 씬 이름을 넣어야함

        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    private void OnPause()
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

    public void SetMasterVolume()
    {
        float sound = soundSlider.value;

        if (sound == -40f) mainAudioMixer.SetFloat("Master", -80);
        else mainAudioMixer.SetFloat("Master", sound);
    }
    public void SetBGMVolume()
    {
        float sound = bgmSlider.value;

        if (sound == -40f) mainAudioMixer.SetFloat("BGM", -80);
        else mainAudioMixer.SetFloat("BGM", sound);
    }
    public void SetSFXVolume()
    {
        float sound = sfxSlider.value;

        if (sound == -40f) mainAudioMixer.SetFloat("SFX", -80);
        else mainAudioMixer.SetFloat("SFX", sound);
    }

    public void OnClickGoToTitle() // "타이틀" 버튼 클릭시
    {
        warningPopup.SetActive(true);
    }

    public void ConfirmGoToTitle() // 팝업에서 "예" 선택시
    {
        Time.timeScale = 1f;
        GameManager.Instance.GameOver(); // TODO: 타이틀 씬 이름을 넣어야함
    }

    public void CancelGoToTitle() // 팝업에서 "아니오" 선택시
    {
        warningPopup.SetActive(false);
    }
    public void SetResolution(int resolutionIndex) // 해상도 설정
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
}


