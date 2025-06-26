using UnityEngine;

namespace PHG
{
    /// <summary>
    /// 사다리·로프 오브젝트의 상/하단 기준점을 자동 계산해 주는 헬퍼.<br />
    /// <para>• 에디터 수정 시 <see cref="OnValidate"/> 에서 즉시 위치 재계산</para>
    /// <para>• 런타임 첫 Awake 에서도 한번 더 확인해 확실히 보강</para>
    public class LadderBounds : MonoBehaviour
    {
        [Header("Auto-generated pivot")]
        public Transform bottom;   // 사다리 아래쪽
        public Transform top;      // 사다리 위쪽

        [Header("Debug")]
        [SerializeField] private bool showDebug = false;

        /*------------------------------------------------------------------------*/
        private void Awake() => SetupIfNeeded();
        private void OnValidate() => SetupIfNeeded();

        /*-------------------------------------------------------------------------*/

        ///<summary> Editor / Runtime 모두에서 한 번 호출</summary>
        private void SetupIfNeeded()
        {
            //bottom/top transform 없으면 자동 생성(숨김)
            if(bottom ==null)
            {
                bottom = new GameObject("bottom").transform;
                bottom.SetParent(transform, worldPositionStays: false);
                bottom.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
            }

            if (top == null)
            {
                top = new GameObject("top").transform;
                top.SetParent(transform, worldPositionStays: false);
                top.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
            }

            // 콜라이더 extents를 읽어 두 pivot 위치 갱신
            if (ComputeWorldExtents(out Vector2 min, out Vector2 max))
            {
                float centerX = (min.x + max.x) * 0.5f;          // ← 중심 X
                bottom.position = new Vector3(centerX, min.y, transform.position.z);
                top.position = new Vector3(centerX, max.y, transform.position.z);
            }

        }
        /// <summary>
        /// 콜라이더 world AABB를 반환 </summary>
        /// 
        
        private bool ComputeWorldExtents(out Vector2 min, out Vector2 max)
        {
            Collider2D col = GetComponent<Collider2D>();
            if(col != null)
            {
                Bounds b = col.bounds;
                min = b.min;
                max = b.max;
                return true;
            }
            min = max = default;
            return false;
        }

        private void OnDrawGizmosSelected()
        {
            if (!showDebug || bottom == null || top == null) return;

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(bottom.position, 0.05f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(top.position, 0.05f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(bottom.position, top.position);
        }


    }
}