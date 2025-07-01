using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PHG
{
    public class LootDropper : MonoBehaviour
    {
        [SerializeField] List<GameObject> dropTable;
        [Range(0f, 1f)] public float dropChance = 1f;
        [SerializeField] Vector2 popImpulse = new Vector2(0f, 2f);
        public void Drop()
        {
            if (Random.value > dropChance || dropTable.Count == 0)
            {
                return;
            }
            var prefab = dropTable[Random.Range(0, dropTable.Count)];
            var loot = Instantiate(prefab, transform.position + Vector3.up * 0.2f, Quaternion.identity);

            if (loot.TryGetComponent(out Rigidbody2D rb))
                rb.AddForce(popImpulse, ForceMode2D.Impulse);
        }
    }
}

