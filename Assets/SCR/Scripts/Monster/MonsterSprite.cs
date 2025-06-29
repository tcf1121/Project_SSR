using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCR
{
    public class MonsterPhyical : MonoBehaviour
    {
        [SerializeField] private Transform _transform;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Rigidbody2D _rigid;
        [SerializeField] private BoxCollider2D _collider;
        public Transform Transform { get => _transform; }
        public SpriteRenderer SpriteRenderer { get => _spriteRenderer; }
        public Rigidbody2D Rigid { get => _rigid; }
        public BoxCollider2D Collider { get => _collider; }

        public void SetMonster(MonsterPhyical monster)
        {
            _transform.position = monster.Transform.position;
            _spriteRenderer.sprite = monster.SpriteRenderer.sprite;
            _spriteRenderer.color = monster.SpriteRenderer.color;
            _rigid.gravityScale = monster.Rigid.gravityScale;
            _collider.size = monster.Collider.size;
        }
    }
}

