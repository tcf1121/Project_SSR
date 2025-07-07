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
        GameManager.Instance.GameOver();
    }

    public void PlayerDead()
    {
        stopGamePanel.SetActive(true);
        Time.timeScale = 0f; // 게임 시간을 0으로 만들어 일시정지
        
    }
    private void Start()
    {
        
    }




    private void OnEnable()
    {
        int level = GameManager.Player.PlayerStats.Level;
        float danger = DangerIndexManager.Instance.GetDangerIndex();
        float money = GameManager.Player.PlayerStats.Money;
        float time = DangerIndexManager.Instance.GetCount();

        result.text = string.Format($"Level :{level}\t\t Danger : {danger}\t\t Money : {money}\t\t Time : {time}");
    }
}
