using UnityEngine;
using Utill;

namespace SCR
{
    public class Box : InteractionObject
    {
        [SerializeField] GameObject MimicPrefab;

        public override void Interaction()
        {
            if (!_isOpen)
            {
                if (Random.Range(0, 2) == 0)
                {
                    _animator.SetTrigger("Open");
                    Use();
                }

                else
                {
                    GameObject monster = ObjectPool.TakeFromPool(EPoolObjectType.CDMonster);
                    monster.GetComponent<Monster>().Clone(MimicPrefab.GetComponent<Monster>());
                    monster.transform.position = gameObject.transform.position;
                    ObjectPool.ReturnPool(this.gameObject, EPoolObjectType.Object);
                }

                _isOpen = true;
            }
        }

        public override void Use()
        {
            GameManager.StageManager.ItemSpawner.SetPos(gameObject.transform.position);
            GameManager.StageManager.ItemSpawner.Spawn();
            ObjectPool.ReturnPool(this.gameObject, EPoolObjectType.Object);
        }
    }
}

