using UnityEngine;
using Utill;
namespace SHL
{
    public class PointBrige : MonoBehaviour
    {
        [SerializeField] private Spwaner spwaner;
        [SerializeField] private RandomCreater randomCreater;
        void Start()
        {
            if (spwaner == null)
            {
                spwaner = GetComponent<Spwaner>();
            }
            if (randomCreater == null)
            {
                randomCreater = GetComponent<RandomCreater>();
            }
            Brige();
        }

        void Brige()
        {
            if (spwaner.SpwanPoint.Count == 0)
            {
                spwaner.SpwanPoint = randomCreater._transformList;
            }
            
        }

       
    }
}
