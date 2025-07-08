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
        if (collision.gameObject.CompareTag("Boss"))
        {
            if (collision.gameObject.GetComponent<BODController>())
                collision.gameObject.GetComponent<BODController>().TakeDamage((int)(weapon.Player.PlayerStats.FinalStats.Atk * weapon.DamageRatio));
            else if (collision.gameObject.GetComponent<GolemController>())
                collision.gameObject.GetComponent<GolemController>().TakeDamage((int)(weapon.Player.PlayerStats.FinalStats.Atk * weapon.DamageRatio));
            // else if (collision.gameObject.GetComponent<CrystalKnightController>())
            // collision.gameObject.GetComponent<CrystalKnightController>().TakeDamage((int)(weapon.Player.PlayerStats.FinalStats.Atk * weapon.DamageRatio));
            return;
        }


    }
}
