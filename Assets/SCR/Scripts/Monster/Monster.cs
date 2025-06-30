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
        public int Hp { get => _hp; }
        [SerializeField] private int _hp;
        public int Damage { get => _damage; }
        [SerializeField] private int _damage;
        public MonsterPhyical Sprite { get => _sprite; }
        [SerializeField] private MonsterPhyical _sprite;

        public void SetMonster(Monster monster)
        {
            _isGround = monster.IsGround;
            _creadit = monster.Credit;
            _hp = monster.Hp;
            _damage = monster.Damage;
            _sprite.SetMonster(monster.Sprite);
        }
    }
}

