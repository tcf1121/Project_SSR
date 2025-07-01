using UnityEngine;

namespace SCR
{
    public class Box : MonoBehaviour
    {

        private Animator _animator; // 애니메이터 컴포넌트를 참조하기 위한 변수
        private bool _isOpen = false; //상자가 열려있는지 여부

        private void Start()
        {
            _animator = GetComponent<Animator>(); // 애니메이터 컴포넌트를 가져옴
            _isOpen = false;
        }
        /// <summary>
        /// 상자를 여는 메서드
        /// </summary>
        #region 
        public void BoxOpen()
        {
            if (!_isOpen)
            {
                _animator.SetTrigger("Open");
                _isOpen = true;

            }

        }
        #endregion


        public void GetItem()
        {
            Debug.Log("아이템 뽑는 중");
            GameManager.StageManager.ItemSpawner.SetPos(gameObject.transform.position);
            GameManager.StageManager.ItemSpawner.Spawn();
        }
    }
}

