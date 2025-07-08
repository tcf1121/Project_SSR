

using UnityEngine;

public class Chest : InteractionObject
{
    
    
    
    public override void Interaction()
    {
        if (!_isOpen && GameManager.Player.UseMoney(25))
        {
            _animator.SetTrigger("Open");
            _isOpen = true;
            Use();
        }
    }

    public override void Use()
    {
        SoundManager.Instance.PlaySFX("Box_Open");
        Debug.Log(GameManager.StageManager);
        Vector2 pos = gameObject.transform.position;
        pos.y += 0.5f;
        GameManager.StageManager.ItemSpawner.SetPos(pos);
        GameManager.StageManager.ItemSpawner.Spawn();
    }
}