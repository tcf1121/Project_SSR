using UnityEngine;
using Utill;

namespace SCR
{
    public class BlueTotem : InteractionObject
    {
        public override void Interaction()
        {
            if (!_isOpen && GameManager.Player.UseMoney(30))
            {
                _animator.SetTrigger("Open");
                _isOpen = true;
            }
        }

        public override void Use()
        {
            // 3개의 랜덤 아이템 중 선택할 수 있게하기
            ObjectPool.ReturnPool(this.gameObject, EPoolObjectType.Object);
        }
    }
}

