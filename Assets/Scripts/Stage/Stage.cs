using UnityEngine;

public class Stage : MonoBehaviour
{
    public int StageNum { get => _stageNum; }
    [SerializeField] private int _stageNum;
    public GameObject SkyObj { get => _skyObj; }
    [SerializeField] private GameObject _skyObj;

    private AudioSource audioSource;
    [SerializeField] private AudioClip stage1;
    [SerializeField] private AudioClip stage2;
    [SerializeField] private AudioClip stage3;
    [SerializeField] private AudioClip stage4;


    private void Start()
    {

        audioSource.clip = Selectauido(StageNum);
        audioSource.Play();
    }

    AudioClip Selectauido(int number)
    {
        if(number == 1) return stage1;
        else if(number == 2) return stage2;
        else if(number == 3) return stage3;
        else return stage4;
        
    }
}

