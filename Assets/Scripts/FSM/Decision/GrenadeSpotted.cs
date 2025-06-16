using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Decisions/GrenadeSpotted")]
public class GrenadeSpotted : Decision
{
    public override bool Decide(FSMEnemy stateMachine)
    {
        var visibleGrenades = stateMachine.fieldOfView.visibleGrenades;

        return (visibleGrenades.Count > 0) ? true : false;
    }
}
