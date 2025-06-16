using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Decisions/SearchTimeEnd")]
public class SearchTimeEnd : Decision
{
    public override bool Decide(FSMEnemy stateMachine)
    {
        var timeSearch = stateMachine.timeSearch;
        var timeSearchMax = stateMachine.timeSearchMax;

        return (timeSearch >= timeSearchMax) ? true : false;
    }
}
