using Utill;

public class Obelisk : InteractionObject
{
    public override void Interaction()
    {
        if (!_isOpen && GameManager.Player.Equipped.CheckSlot())
        {
            if (GameManager.Player.UseObelisk())
            {

                _animator.SetTrigger("Open");
                _isOpen = true;
            }
        }
    }

    public override void Use()
    {
        GameManager.Player.Equipped.ExpansionSlot();
        ObjectPool.ReturnPool(this.gameObject, EPoolObjectType.Object);
    }
}