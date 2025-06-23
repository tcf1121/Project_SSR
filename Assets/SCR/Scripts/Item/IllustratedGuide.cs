using System.Collections.Generic;
using UnityEngine;

namespace SCR
{
    public class IllustratedGuide : MonoBehaviour
    {
        public List<GameObject> AllItem;
        public List<GameObject> UnlockItem;
        public List<GameObject> LockItem;

        public void Unlock(GameObject item)
        {
            AllItem.Find(n => n == item);
            LockItem.Remove(item);
            UnlockItem.Add(item);
        }

        public GameObject DropItem()
        {
            return UnlockItem[Random.Range(0, UnlockItem.Count)];
        }
    }
}


