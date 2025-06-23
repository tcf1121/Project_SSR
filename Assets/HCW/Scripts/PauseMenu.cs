using UnityEngine;
using UnityEngine.UI;

namespace Util
{
    public class PauseMenu : MonoBehaviour
    {
        public GameObject pauseMenuPanel;
        public Slider soundSlider;

        private bool isPaused = false;

        private void Start()
        {
            pauseMenuPanel.SetActive(false);
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                
            }
        }
    }
}


