using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleUI : MonoBehaviour
{
    [SerializeField] Button startBtn;
    [SerializeField] Button endBtn;
    [SerializeField] Button setBtn;
    [SerializeField] Button dicBtn;

    void Start()
    {
        startBtn.onClick.AddListener(GameManager.Instance.StartGame);
        endBtn.onClick.AddListener(GameManager.Instance.EndGame);
        setBtn.onClick.AddListener(GameManager.Instance.PauseMenu);
        //dicBtn.onClick.AddListener();
    }

}
