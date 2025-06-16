using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Decisions/GrenadeNotNearChase")]
public class GrenadeNotNearChase : Decision
{
    public override bool Decide(FSMEnemy stateMachine)
    {
        var visibleGrenades = stateMachine.fieldOfView.visibleGrenades;
        var visibleTargets = stateMachine.fieldOfView.visibleTargets;

        var fieldOfView = stateMachine.fieldOfView;

        int aliveVisibleEnemy = 0;

        foreach (var enemy in fieldOfView.visibleTargets)
        {
            if (!enemy.gameObject.GetComponentInParent<ITarget>().isDead)
            {
                aliveVisibleEnemy++;
            }
        }

        return (visibleGrenades.Count == 0 && aliveVisibleEnemy > 0) ? true : false;
    }
}
