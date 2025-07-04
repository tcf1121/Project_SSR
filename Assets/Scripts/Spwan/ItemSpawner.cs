using UnityEngine;
using Utill;

public class ItemSpawner : MonoBehaviour
{
    private Vector2 _spwanPoint;

    private void Awake() => Init();

    private void Init()
    {
    }

    public void SetPos(Vector2 pos)
    {
        _spwanPoint = pos;
    }

    public void Spawn()
    {
        GameManager.ItemManager.PickItem();
        bool IsAttackItem = GameManager.ItemManager.IsAttackItem();
        GameObject item;
        if (IsAttackItem)
        {
            item = ObjectPool.TakeFromPool(EPoolObjectType.AttackItem);
            item.GetComponent<AttackItem>().Clone(GameManager.ItemManager.AttackItem);
            item.transform.position = _spwanPoint;
            item.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 2f, ForceMode2D.Impulse);
        }
        else
        {
            item = ObjectPool.TakeFromPool(EPoolObjectType.StatItem);
            item.GetComponent<StatItem>().Clone(GameManager.ItemManager.StatItem);
            item.transform.position = _spwanPoint;
            item.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 2f, ForceMode2D.Impulse);
        }
    }

    public void Spawn(GameObject itemObj)
    {
        bool IsAttackItem = itemObj.GetComponent<AttackItem>() != null ? true : false;
        GameObject item;
        if (IsAttackItem)
        {
            item = ObjectPool.TakeFromPool(EPoolObjectType.AttackItem);
            item.GetComponent<AttackItem>().Clone(itemObj.GetComponent<AttackItem>());
            item.transform.position = _spwanPoint;
            item.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 2f, ForceMode2D.Impulse);
        }
        else
        {
            item = ObjectPool.TakeFromPool(EPoolObjectType.StatItem);
            item.GetComponent<StatItem>().Clone(itemObj.GetComponent<StatItem>());
            item.transform.position = _spwanPoint;
            item.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 2f, ForceMode2D.Impulse);
        }
    }
}

