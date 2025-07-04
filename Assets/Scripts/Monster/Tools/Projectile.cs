using UnityEngine;

/// <summary>
/// ���� �߻�ü ? �浹������ ���� �� Ǯ�� ����
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    //�������
    [SerializeField] private float speed = 10f; //�̵��ӵ�
    [SerializeField] private float lifeTime;//�ִ� ���� �ð�
    private MonsterBrain brain;
    private int projectileDamage;

    private Rigidbody2D rb;
    public int PoolKey { get; set; }
    // ��Ÿ��
    private float alive;
    //����׿�
    private Vector2 lastDir;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Debug.Log($"[Projectile] Awake: name={gameObject.name}, rb={rb}, active={gameObject.activeSelf}");

    }
    private void OnEnable()
    {
        alive = 0f;
        lastDir = Vector2.right; //  �⺻ �������� �ʱ�ȭ
        transform.rotation = Quaternion.identity; // 
        Debug.Log($"[Projectile] OnEnable ȣ���, lifeTime={lifeTime}");
    }
    public void Launch(Vector2 dir, float newSpeed, MonsterBrain monsterBrain)
    {
        this.brain = monsterBrain;

        // brain �Ǵ� StatData�� ��ȿ���� Ȯ��
        if (this.brain == null || this.brain.StatData == null)
        {
            Debug.LogError($"[Projectile {gameObject.name}] Launch() ȣ�� �� MonsterBrain �Ǵ� StatData�� NULL�Դϴ�. �߻� �ߴ�.", this);
            ReturnToPool(); // �ʼ� �����Ͱ� ������ Ǯ�� ��� ��ȯ
            return;
        }

        // lifeTime�� speed�� monsterBrain.StatData���� ������ ����
        lifeTime = this.brain.StatData.projectileLife;
        this.speed = newSpeed; // newSpeed�� monsterBrain.StatData.projectileSpeed���� �����;� ��.
        projectileDamage = this.brain.StatData.damage; // ������ ���ݷ��� ����ü �������� ����


        // Rigidbody2D �ʱ�ȭ (���� ���� ����)
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
            Debug.Log("Awake, OnEnable�ܰ迡�� rb�޾ƿ��� ����");
            if (rb == null)
            {
                Debug.LogError($"[Projectile {gameObject.name}] Launch() ��õ� �Ŀ��� Rigidbody2D�� ������ NULL�Դϴ�. �߻� �ߴ�.", this);
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

        //�ǽð� ���� ����
        Debug.DrawRay(transform.position, lastDir * 0.5f, Color.cyan, 0.1f);
        alive += Time.deltaTime;
        if (alive >= lifeTime) ReturnToPool();
    }


    private void OnTriggerEnter2D(Collider2D col)
    {
        // �÷��̾� �±� Ȯ�� �� PlayerStats ������Ʈ ����
        if (col.gameObject.CompareTag("Player"))
        {
            Debug.Log($"{gameObject.name}] �÷��̾�� �浹");
            PlayerStats playerStats = col.gameObject.GetComponent<PlayerStats>();
            // �÷��̾ ã������ ������ ����
            playerStats.TakeDamage(projectileDamage); // ������ ���ݷ�(projectileDamage) ����
            Debug.Log($"[Projectile - {gameObject.name}] {col.name} (�÷��̾�)���� {projectileDamage} ������ ����!");
            ReturnToPool(); // �÷��̾�� �������� �־����� Ǯ�� ����


        }
        // TODO: �ٸ� ������Ʈ (��, �� ��)�� �浹 �� ó�� ���� �߰�
        // ���� ���, Projectile�� ���� �ε����� ��������� �Ϸ���:
        // else if (col.CompareTag("Wall"))
        // {
        //     ReturnToPool();
        // }
        // Ȥ�� Ư�� ���̾�(��: Ground, Environment)�� �浹 ��:
        // if (((1 << col.gameObject.layer) & brain.StatData.groundMask) != 0) // ����: GroundMask�� Ȱ��
        // {
        //     ReturnToPool();
        // }
    }

    private void ReturnToPool()
    {
        // Ǯ�� ��ȯ�ϱ� ���� Rigidbody2D�� velocity�� 0���� �����Ͽ� �ܷ� �������� ����
        // �� �������� rb�� null�� �� �����Ƿ� null üũ
        if (rb == null) Debug.LogWarning($"[Projectile {gameObject.name}] ReturnToPool() �� Rigidbody2D�� NULL�Դϴ�. Velocity�� 0���� ������ �� �����ϴ�.", this);
        else rb.velocity = Vector2.zero;

        ProjectilePool.Instance.Release(this); // Ǯ�� ����ü ��ȯ
    }



}
