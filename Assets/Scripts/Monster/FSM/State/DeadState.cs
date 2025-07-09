using System.Collections;
using UnityEngine;
using Utill;


public class DeadState : IState
{
    private Monster _monster;

    public DeadState(Monster monster)
    {
        _monster = monster;
    }

    public void Enter()
    {
        if (_monster.AudioSource != null && _monster.deathSoundClip != null)
        {
            _monster.AudioSource.PlayOneShot(_monster.deathSoundClip);
        }
        _monster.Death();
    }

    public void Tick() { }

    public void Exit() 
    {
        _monster.Brain.SetIsDead(false);
       

    }


}