using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Decisions/TakeDamage")]
public class TakeDamageDecision : Decision
{
    public override bool Decide(FSMEnemy stateMachine)
    {
        return stateMachine.takenDamage;
    }
}
