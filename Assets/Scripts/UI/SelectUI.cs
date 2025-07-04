using System.Collections.Generic;
using UnityEngine;

public class SelectUI : MonoBehaviour
{
    public List<SelectItemUI> SeletList { get => _seletList; }
    [SerializeField] private List<SelectItemUI> _seletList;
    private GameObject _useTotem;

    public void SetItem(List<AttackItem> attackItems, List<StatItem> statItems)
    {
        for (int i = 0; i < 3; i++)
        {
            if (attackItems[i] != null) _seletList[i].SetAttackItem(attackItems[i]);
            else _seletList[i].SetStatItem(statItems[i]);
        }
    }

    public void SeletItem(int index)
    {
        GameManager.Player.Equip(_seletList[index].GetItem());
        if (_useTotem.TryGetComponent<BlueTotem>(out BlueTotem Totem))
            Totem.Disappear();
        OnOffUI(false);
    }

    public void SetTotem(GameObject gameObject)
    {
        _useTotem = gameObject;
    }

    public void OnOffUI(bool onoff)
    {
        if (onoff)
        {
            Time.timeScale = 0f;
            gameObject.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            gameObject.SetActive(false);
        }
    }

}
