using UnityEngine;

namespace SCR
{
    public class Item : MonoBehaviour
    {
        // 아이템 부위
        public ItemPart ItemPart { get { return _itemPart; } }
        [SerializeField] private ItemPart _itemPart;

        // 아이템 이름
        public string Name { get { return _name; } }
        [SerializeField] private string _name;

        // 아이템 설명
        public string Description { get { return _description; } }
        [SerializeField] private string _description;
    }
    public enum ItemPart
    {
        Head,
        Arm,
        Foot,
        Body
    }
}

