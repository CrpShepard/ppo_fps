using System.Collections;
using UnityEngine;

public class BaseState : ScriptableObject
{
    public enum State
    {
        Patrol,
        Chase,
        Search,
        Hide,
        PickupItem,
        EvadeGrenade
    } 

    public virtual void Execute(FSMEnemy machine) { }
}
