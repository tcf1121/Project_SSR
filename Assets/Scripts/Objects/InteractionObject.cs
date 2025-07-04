using UnityEngine;


public abstract class InteractionObject : MonoBehaviour
{
    protected Animator _animator; // 애니메이터 컴포넌트를 참조하기 위한 변수
    protected bool _isOpen = false; //상자가 열려있는지 여부

    private void Start()
    {
        _animator = GetComponent<Animator>(); // 애니메이터 컴포넌트를 가져옴
        _isOpen = false;
    }
    /// <summary>
    /// 상호 작용하는 메서드
    /// </summary>
    public abstract void Interaction();

    /// <summary>
    /// 사용했을 때 일어날 일
    /// </summary>
    public abstract void Use();
}

