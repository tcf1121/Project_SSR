using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtttackBox : MonoBehaviour
{
    [SerializeField] private int _damage;

    public void SetDamage(int damage)
    {
        _damage = damage;
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GameManager.Player.TakeDamage(_damage);
        }
    }
}
