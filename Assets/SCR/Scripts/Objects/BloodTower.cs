using UnityEngine;
using Utill;

namespace SCR
{
    public class BloodTower : InteractionObject
    {
        int gold;
        public override void Interaction()
        {
            if (!_isOpen)
            {
                float ratio = 0.1f + 0.2f * Random.Range(0, 4);
                gold = GameManager.Player.UseCurrentHpRatio(ratio);
                _animator.SetTrigger("Open");
                _isOpen = true;

            }
        }

        public override void Use()
        {
            GameManager.Player.GetReward(gold, 0);
            ObjectPool.ReturnPool(this.gameObject, EPoolObjectType.Object);
        }
    }
}

