using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PHG
{
    // / <summary>
    // 단순 가변- 사이즈 오브젝트 풀(DontDestroyOnLoad)
    //</summary>

    public class ProjectilePool : MonoBehaviour
    {
        [SerializeField] private Projectile prefab;
        [SerializeField] private int prewarm = 32;
        private readonly Queue<Projectile> pool = new();

        //싱글턴
        public static ProjectilePool Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            for (int i = 0; i < prewarm; i++) CreateNew();
        }

        private Projectile CreateNew()
        {
            Projectile p = Instantiate(prefab, transform);
            p.gameObject.SetActive(false);
            pool.Enqueue(p);
            return p;
        }

        public Projectile Get()
        {
            if (pool.Count == 0) CreateNew();
            Projectile p = pool.Dequeue();
            p.gameObject.SetActive(true);
            return p;
        }

        public void Return(Projectile p)
        {
            p.gameObject.SetActive(false);
            pool.Enqueue(p);
        }
    }
}
