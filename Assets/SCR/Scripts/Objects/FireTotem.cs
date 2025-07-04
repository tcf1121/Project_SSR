using UnityEngine;
using Utill;

namespace SCR
{
    public class FireTotem : BlueTotem
    {
        public override void Interaction()
        {
            if (!_isOpen)
            {
                float ratio = 0.1f + 0.2f * Random.Range(0, 4);
                GameManager.Player.UseCurrentHpRatio(ratio);
                _isOpen = true;
                Use();
            }
            if (_isOpen)
            {
                GameManager.Player.ConditionalUI.SelectUI.SetItem(_attackItems, _statItems);
                GameManager.Player.ConditionalUI.SelectUI.SetTotem(gameObject);
                GameManager.Player.ConditionalUI.SelectUI.OnOffUI(true);
            }
        }
    }
}

