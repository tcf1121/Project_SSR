using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadRandom : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] TMP_Text subtitle;
    [SerializeField] TMP_Text des;

    private void Awake()
    {
        int RandNum = Random.Range(0, 6);
        animator.SetInteger("Random", RandNum);
        SetText(RandNum);
    }

    private void SetText(int randNum)
    {
        switch (randNum)
        {
            case 0:
                subtitle.text = "상호작용";
                des.text = "맵에 있는 오브젝트들과 상호작용을 하기 위해선 F키를 입력하면 됩니다.";
                break;
            case 1:
                subtitle.text = "텔레포터";
                des.text = "충분히 강하다면 텔레포터에서 보스를 소환해보세요.";
                break;
            case 2:
                subtitle.text = "장비";
                des.text = "장비는 부위마다 장착할 수 있는 한계가 있습니다. 하지만 다리는 한계가 없죠.";
                break;
            case 3:
                subtitle.text = "운";
                des.text = "상자를 열어 운을 테스트 해보세요. 운이 없다면 깜짝 놀랄겁니다.";
                break;
            case 4:
                subtitle.text = "기본 공격";
                des.text = "기본 공격은 다른 키와 다르게 계속 누르고 있어야 사용이 됩니다.";
                break;
            case 5:
                subtitle.text = "클리어";
                des.text = "보스를 잡아도 90초 동안은 생존하셔야 클리어할 수 있어요.";
                break;

        }
    }
}
