using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SCR
{
    public enum MonsterType
    {
        CDMonster,
        LDMonster,
        FlyMonster
    }
    public class Monster : MonoBehaviour
    {
        public MonsterType MonsterType { get => _monsterType; }
        [SerializeField] private MonsterType _monsterType;
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
        public GameObject MuzzlePoint { get => _muzzlePoint; }
        [SerializeField] private GameObject _muzzlePoint;


        public void Clone(Monster monster)
        {
            _hitBox.offset = monster.HitBox.offset;
            _hitBox.isTrigger = monster.HitBox.isTrigger;
            _hitBox.size = monster.HitBox.size;
            _hitBox.edgeRadius = monster.HitBox.edgeRadius;
            _sprite.sprite = monster.Sprite.sprite;
            _sprite.flipX = monster.Sprite.flipX;
            _animator.runtimeAnimatorController = monster.Animator.runtimeAnimatorController;
            _monsterBrain.Clone(monster.MonsterBrain);
            _monsterBrain.enabled = true;
            _monsterStats.enabled = true;
            _monsterType = monster.MonsterType;
            _creadit = monster.Credit;
            if (MonsterType == MonsterType.LDMonster)
            {
                _muzzlePoint.transform.position = monster.MuzzlePoint.transform.position;
            }
        }
    }
}

