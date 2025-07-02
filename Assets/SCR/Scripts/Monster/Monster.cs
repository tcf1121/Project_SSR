using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SCR
{
    public class Monster : MonoBehaviour
    {
        public bool IsGround { get => _isGround; }
        [SerializeField] private bool _isGround;
        public int Credit { get => _creadit; }
        [SerializeField] private int _creadit;


        public PHG.MonsterBrain MonsterBrain { get => _monsterBrain; }
        [SerializeField] private PHG.MonsterBrain _monsterBrain;
        public PHG.MonsterStats MonsterStats { get => _monsterStats; }
        [SerializeField] private PHG.MonsterStats _monsterStats;
        public BoxCollider2D HitBox { get => _hitBox; }
        [SerializeField] private BoxCollider2D _hitBox;
        public SpriteRenderer Sprite { get => _sprite; }
        [SerializeField] private SpriteRenderer _sprite;
        public Animator Animator { get => _animator; }
        [SerializeField] private Animator _animator;


        public void Clone(Monster monster)
        {
            _hitBox.offset = monster.HitBox.offset;
            _hitBox.isTrigger = monster.HitBox.isTrigger;
            _hitBox.size = monster.HitBox.size;
            _hitBox.edgeRadius = monster.HitBox.edgeRadius;
            _sprite.sprite = monster.Sprite.sprite;
            _animator.runtimeAnimatorController = monster.Animator.runtimeAnimatorController;
            _monsterBrain.Clone(monster.MonsterBrain);
            _monsterBrain.enabled = true;
            _monsterStats.enabled = true;
            _isGround = monster.IsGround;
            _creadit = monster.Credit;
        }
    }
}

