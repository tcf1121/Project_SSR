using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SCR
{
    public class LoadingSceneManager : MonoBehaviour
    {
        public static int nextScene;
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TMP_Text _percentText;
        public static Coroutine loadCor;

        private void Start()
        {
            loadCor = StartCoroutine(LoadScene());
        }



        public static void LoadScene(int sceneNum)
        {
            nextScene = sceneNum;
            SceneManager.LoadScene(1);
        }

        IEnumerator LoadScene()
        {
            yield return null;
            AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
            op.allowSceneActivation = false;
            float timer = 0.0f;
            while (!op.isDone)
            {
                yield return null;
                timer += Time.deltaTime;
                if (op.progress < 0.9f)
                {
                    _progressBar.value = Mathf.Lerp(_progressBar.value, op.progress, timer);
                    _percentText.text = (_progressBar.value * 100).ToString("F2");
                    if (_progressBar.value >= op.progress)
                    {
                        timer = 0f;
                    }
                }
                else
                {
                    _progressBar.value = Mathf.Lerp(_progressBar.value, 1f, timer);
                    _percentText.text = (_progressBar.value * 100).ToString("F2");
                    if (_progressBar.value == 1.0f)
                    {
                        op.allowSceneActivation = true;
                        if (nextScene == 2)
                        {

                        }
                        else if (nextScene == 3)
                        {

                        }

                        StopCoroutine(loadCor);
                        loadCor = null;
                        yield break;

                    }
                }
            }

        }
    }

}
