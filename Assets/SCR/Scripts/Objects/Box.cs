using UnityEngine;

namespace SCR
{
    public class Box : MonoBehaviour
    {

        private Animator _animator; // �ִϸ����� ������Ʈ�� �����ϱ� ���� ����
        private bool _isOpen = false; //���ڰ� �����ִ��� ����

        private void Start()
        {
            _animator = GetComponent<Animator>(); // �ִϸ����� ������Ʈ�� ������
            _isOpen = false;
        }
        /// <summary>
        /// ���ڸ� ���� �޼���
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
            //StageManager �ȿ� ItemSpawner�� �־ ���� �� �� �ְ� ��.
            // GameManger.StageManger.ItemSpawner.Spawn(Vector2 ��ġ);


        }
    }
}

