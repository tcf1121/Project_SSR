using UnityEngine;
using Utill;

namespace SCR
{
    public class FireTotem : InteractionObject
    {
        public override void Interaction()
        {
            if (!_isOpen)
            {
                float ratio = 0.1f + 0.2f * Random.Range(0, 4);
                GameManager.Player.UseCurrentHpRatio(ratio);
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

