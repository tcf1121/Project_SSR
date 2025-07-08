using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject _playerPrefab;
    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }
    public static UnityAction<ItemPart, int> SelectEvent;
    public static Player Player { get { return _player; } }
    private static Player _player;
    public static int Stage { get => _stage; }
    private static int _stage;
    public static StageManager StageManager { get => _stageManager; }
    private static StageManager _stageManager;
    public static ItemManager ItemManager { get => _itemManager; }
    private static ItemManager _itemManager;
    [SerializeField] private DeadUi deadUi;
    [SerializeField] private PauseMenu pauseMenu;

    private void Awake()
    {
        SetSingleton();
        _stage = 1;
        _itemManager = GetComponent<ItemManager>();
    }
    private void SetSingleton()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            TestStage();
            GameObject PlayerGO = Instantiate(_playerPrefab);
            PlayerGO.transform.parent = this.transform;
            _player = PlayerGO.GetComponent<Player>();
        }
    }

    private void OnEnable()
    {

    }

    public void GameOver()
    {
        deadUi.PlayerDead();

    }

    public void EndGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void PauseMenu()
    {
        pauseMenu.PauseGame();
    }


    public void StartGame()
    {
        NextStage();
        GameObject PlayerGO = Instantiate(_playerPrefab);
        PlayerGO.transform.parent = this.transform;
        _player = PlayerGO.GetComponent<Player>();

    }

    public static void StageClear()
    {
        _stage++;
        if (_stage > 4)
            _stage = 4;
    }

    public static void SetStageManager(StageManager stageManager)
    {
        _stageManager = stageManager;
    }

    public static void NextStage()
    {
        LoadingSceneManager.LoadScene(2);
    }

    private void TestStage()
    {
        LoadingSceneManager.LoadScene(3);
    }
}

