using UnityEngine;

public class AttackTrigger : MonoBehaviour
{
    [SerializeField] private Weapon weapon;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Monster"))
        {
            collision.gameObject.GetComponent<Monster>().GetDamage((int)(weapon.Player.PlayerStats.FinalStats.Atk * weapon.DamageRatio));
            return;
        }
    }
}
