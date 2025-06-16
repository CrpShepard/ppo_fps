using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Decisions/HideTimeEnd")]
public class HideTimeEnd : Decision
{
    public override bool Decide(FSMEnemy stateMachine)
    {
        var hideSearch = stateMachine.timeHide;
        var hideSearchMax = stateMachine.timeHideMax;

        return (hideSearch >= hideSearchMax) ? true : false;
    }
}
