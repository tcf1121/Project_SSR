using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 프리팹-키 기반 가변 사이즈 Projectile 풀. (싱글턴 / DontDestroyOnLoad)
/// </summary>
public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance { get; private set; }

    /* key = prefab.GetInstanceID() */
    private readonly Dictionary<int, Queue<Projectile>> pools = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);    // 씬 전환 시 유지
    }

    /* ───────────────────────────────────
       public API
       ───────────────────────────────────*/

    /// <param name="prefab">꺼내고 싶은 투사체 프리팹</param>
    /// <param name="spawnPos">생성 위치</param>
    public Projectile Get(Projectile prefab, Vector2 spawnPos)
    {
        if (prefab == null)
        {
            Debug.LogError("ProjectilePool.Get() 호출됨, BUT prefab == NULL!!");
            return null;
        }

        int key = prefab.GetInstanceID();
        Debug.Log($"[ProjectilePool] Instantiating projectile: {prefab.name}, activeSelf={prefab.gameObject.activeSelf}");
        if (!pools.TryGetValue(key, out Queue<Projectile> q))
        {
            Debug.Log("!pools");
            q = new Queue<Projectile>();
            pools[key] = q;
        }

        Projectile proj = q.Count > 0
            ? q.Dequeue()
            : Instantiate(prefab, transform);  // 부모를 풀 오브젝트로 지정

        proj.PoolKey = key;
        proj.transform.position = spawnPos;
        proj.gameObject.SetActive(true);
        return proj;
    }

    /// <summary>
    /// Projectile 자체에서 Hit / 수명 종료 시 호출
    /// </summary>
    public void Release(Projectile proj)
    {
        int key = proj.PoolKey;

        if (!pools.TryGetValue(key, out Queue<Projectile> q))
        {
            Debug.LogWarning($"[ProjectilePool] Unknown PoolKey {key}. Creating fallback queue.");
            q = new Queue<Projectile>();
            pools[key] = q;
        }

        proj.gameObject.SetActive(false);
        q.Enqueue(proj);   // 중복 Enqueue 방지 – 한 번만
    }
}