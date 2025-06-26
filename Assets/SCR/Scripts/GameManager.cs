using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace SCR
{
    public class GameManager : MonoBehaviour
    {

        private static GameManager instance;
        public static GameManager Instance { get { return instance; } }
        public static UnityAction<ItemPart, int> SelectEvent;

        private void Awake()
        {
            SetSingleton();

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

        private void OnEnable()
        {

        }

        private void GameOver()
        {

        }

        public void EndGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                            Application.Quit();
#endif
        }

        private void Update()
        {

        }

        public void StartGame()
        {
            LoadingSceneManager.LoadScene(2);
        }
    }

}
