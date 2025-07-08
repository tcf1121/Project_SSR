using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class DeadUi : MonoBehaviour
{
    public GameObject stopGamePanel;
    public TMP_Text result;
    public void AnykeyGoToLobby()
    {
        Time.timeScale = 1f;
        stopGamePanel.SetActive(false);
        LoadingSceneManager.LoadScene(0);
    }

    public void PlayerDead()
    {
        SetResult();
        stopGamePanel.SetActive(true);

        StartCoroutine(timeCor()); // ���� �ð��� 0���� ����� �Ͻ�����

    }

    private IEnumerator timeCor()
    {
        float time = 1f;
        while (time > 0)
        {
            time -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        Time.timeScale = 0f;
    }


    private void SetResult()
    {
        int level = GameManager.Player.PlayerStats.Level;
        float danger = DangerIndexManager.Instance.GetDangerIndex();
        float money = GameManager.Player.PlayerStats.Money;
        float time = DangerIndexManager.Instance.GetCount();

        result.text = string.Format($"���� :{level}\t\t ���赵 : {danger}\t\t ȹ�� ��� : {money}\t\t �÷��� Ÿ�� : {time}");
    }
}
