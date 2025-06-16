using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Decisions/TargetLost")]
public class TargetLostDecision : Decision
{
    public override bool Decide(FSMEnemy stateMachine)
    {
        var fieldOfView = stateMachine.fieldOfView;

        int aliveVisibleEnemy = 0;

        foreach (var enemy in fieldOfView.visibleTargets)
        {
            if (!enemy.gameObject.GetComponentInParent<ITarget>().isDead)
            {
                aliveVisibleEnemy++;
            }
        }

        return (aliveVisibleEnemy == 0) ? true : false;
    }
}
