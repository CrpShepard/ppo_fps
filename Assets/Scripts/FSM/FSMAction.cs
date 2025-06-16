using UnityEngine;

public abstract class FSMAction : ScriptableObject
{
    public abstract void Execute(FSMEnemy stateMachine);
}
