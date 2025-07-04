using UnityEngine;

/// <summary>
/// 몬스터 발사체 ? 충돌·수명 종료 시 풀로 복귀
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    //멤버변수
    [SerializeField] private float speed = 10f; //이동속도
    [SerializeField] private float lifeTime;//최대 생존 시간
    private MonsterBrain brain;
    private int projectileDamage;

    private Rigidbody2D rb;
    public int PoolKey { get; set; }
    // 런타임
    private float alive;
    //디버그용
    private Vector2 lastDir;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Debug.Log($"[Projectile] Awake: name={gameObject.name}, rb={rb}, active={gameObject.activeSelf}");

    }
    private void OnEnable()
    {
        alive = 0f;
        lastDir = Vector2.right; //  기본 방향으로 초기화
        transform.rotation = Quaternion.identity; // 
        Debug.Log($"[Projectile] OnEnable 호출됨, lifeTime={lifeTime}");
    }
    public void Launch(Vector2 dir, float newSpeed, MonsterBrain monsterBrain)
    {
        this.brain = monsterBrain;

        // brain 또는 StatData가 유효한지 확인
        if (this.brain == null || this.brain.StatData == null)
        {
            Debug.LogError($"[Projectile {gameObject.name}] Launch() 호출 시 MonsterBrain 또는 StatData가 NULL입니다. 발사 중단.", this);
            ReturnToPool(); // 필수 데이터가 없으면 풀로 즉시 반환
            return;
        }

        // lifeTime과 speed를 monsterBrain.StatData에서 가져와 설정
        lifeTime = this.brain.StatData.projectileLife;
        this.speed = newSpeed; // newSpeed는 monsterBrain.StatData.projectileSpeed에서 가져와야 함.
        projectileDamage = this.brain.StatData.damage; // 몬스터의 공격력을 투사체 데미지로 설정


        // Rigidbody2D 초기화 (기존 로직 유지)
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
            Debug.Log("Awake, OnEnable단계에서 rb받아오기 실패");
            if (rb == null)
            {
                Debug.LogError($"[Projectile {gameObject.name}] Launch() 재시도 후에도 Rigidbody2D가 여전히 NULL입니다. 발사 중단.", this);
                return;
            }
        }
        alive = 0f;
        lastDir = dir.normalized;
        this.speed = newSpeed;
        rb.velocity = dir.normalized * this.speed;
        Debug.Log($"[Projectile] Launch! dir={dir}, speed={speed}, rb.vel={rb.velocity}, lifeTime={lifeTime}");
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

    }

    private void Update()
    {

        //실시간 방향 추적
        Debug.DrawRay(transform.position, lastDir * 0.5f, Color.cyan, 0.1f);
        alive += Time.deltaTime;
        if (alive >= lifeTime) ReturnToPool();
    }


    private void OnTriggerEnter2D(Collider2D col)
    {
        // 플레이어 태그 확인 및 PlayerStats 컴포넌트 접근
        if (col.gameObject.CompareTag("Player"))
        {
            Debug.Log($"{gameObject.name}] 플레이어와 충돌");
            PlayerStats playerStats = col.gameObject.GetComponent<PlayerStats>();
            // 플레이어를 찾았으면 데미지 적용
            playerStats.TakeDamage(projectileDamage); // 몬스터의 공격력(projectileDamage) 적용
            Debug.Log($"[Projectile - {gameObject.name}] {col.name} (플레이어)에게 {projectileDamage} 데미지 적용!");
            ReturnToPool(); // 플레이어에게 데미지를 주었으면 풀로 복귀


        }
        // TODO: 다른 오브젝트 (벽, 적 등)와 충돌 시 처리 로직 추가
        // 예를 들어, Projectile이 벽에 부딪히면 사라지도록 하려면:
        // else if (col.CompareTag("Wall"))
        // {
        //     ReturnToPool();
        // }
        // 혹은 특정 레이어(예: Ground, Environment)와 충돌 시:
        // if (((1 << col.gameObject.layer) & brain.StatData.groundMask) != 0) // 예시: GroundMask를 활용
        // {
        //     ReturnToPool();
        // }
    }

    private void ReturnToPool()
    {
        // 풀로 반환하기 전에 Rigidbody2D의 velocity를 0으로 설정하여 잔류 움직임을 방지
        // 이 시점에도 rb가 null일 수 있으므로 null 체크
        if (rb == null) Debug.LogWarning($"[Projectile {gameObject.name}] ReturnToPool() 중 Rigidbody2D가 NULL입니다. Velocity를 0으로 설정할 수 없습니다.", this);
        else rb.velocity = Vector2.zero;

        ProjectilePool.Instance.Release(this); // 풀에 투사체 반환
    }



}
