using UnityEngine;
using SCR; // Player 관련 스크립트의 네임스페이스

namespace HCW
{
    public class BODSpell : MonoBehaviour
    {
        public float damage;
        private float spellDuration = 5f;

        void Start()
        {
            Destroy(gameObject, spellDuration);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player")) // 플레이어 태그 확인
            {
                Player player = other.GetComponent<Player>();
                if (player != null)
                {
                    player.PlayerStats.TakeDamage(damage);
                    Debug.Log($"BODSpell: 플레이어에게 {damage} 데미지를 입혔습니다.");
                }

                Destroy(gameObject);
            }
        }
    }
}
