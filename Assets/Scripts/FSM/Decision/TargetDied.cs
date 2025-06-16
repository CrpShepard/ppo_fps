using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Decisions/TargetDied")]
public class TargetDied : Decision
{
    public override bool Decide(FSMEnemy stateMachine)
    {
        if (stateMachine.currentTarget == null) return false;

        var targetDied = stateMachine.currentTarget.gameObject.GetComponentInParent<ITarget>().isDead;
        return (targetDied);
    }
}
