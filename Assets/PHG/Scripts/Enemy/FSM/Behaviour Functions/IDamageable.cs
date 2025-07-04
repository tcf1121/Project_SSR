using SCR;

namespace PHG 
{
    public interface IDamageable
    {
        void TakeDamage(float amount, UnityEngine.GameObject instigator = null); 
    }
}