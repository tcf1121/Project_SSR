using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utill;


public class SelectItemUI : MonoBehaviour
{
    private AttackItem _attackItem;
    private StatItem _statItem;
    private GameObject _useTotem;

    [SerializeField] private TMP_Text itemText;
    [SerializeField] private TMP_Text desText;
    [SerializeField] private Image itemImage;

    public void SetAttackItem(AttackItem attackItem)
    {
        _attackItem = attackItem;
        itemText.text = _attackItem.Name;
        desText.text = _attackItem.Description;
        itemImage.sprite = _attackItem.Image;
    }

    public void SetStatItem(StatItem statItem)
    {
        _statItem = statItem;
        itemText.text = statItem.Name;
        desText.text = statItem.Description;
        itemImage.sprite = statItem.Image;
    }


    public GameObject GetItem()
    {
        GameObject item;
        if (_attackItem != null)
        {
            item = ObjectPool.TakeFromPool(EPoolObjectType.AttackItem);
            item.GetComponent<AttackItem>().Clone(_attackItem);
            item.SetActive(false);
        }
        else
        {
            item = ObjectPool.TakeFromPool(EPoolObjectType.StatItem);
            item.GetComponent<StatItem>().Clone(_statItem);
            item.SetActive(false);
        }
        return item;
    }
}