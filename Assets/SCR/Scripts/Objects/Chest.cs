using UnityEngine;

namespace SCR
{
    public class Chest : InteractionObject
    {
        public override void Interaction()
        {
            if (!_isOpen && GameManager.Player.UseMoney(25))
            {
                _animator.SetTrigger("Open");
                _isOpen = true;

            }
        }

        public override void Use()
        {
            GameManager.StageManager.ItemSpawner.SetPos(gameObject.transform.position);
            GameManager.StageManager.ItemSpawner.Spawn();
        }
    }
}

