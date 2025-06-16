using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "FSM/Actions/Patrol")]
public class PatrolAction : FSMAction
{
    public override void Execute(FSMEnemy stateMachine)
    {
        var navMeshAgent = stateMachine.navMeshAgent;

        stateMachine.StopAttackEnemy();
        stateMachine.isSearching = false;
        stateMachine.StopHide();
        stateMachine.isPickingUpItem = false;
        stateMachine.itemPickuped = false;
        stateMachine.timeHide = 0f;
        stateMachine.timeSearch = 0f;

        if (stateMachine.IsWalkPointReached() || !stateMachine.isPatrolling) 
        {
            stateMachine.isPatrolling = true;
            navMeshAgent.SetDestination(stateMachine.RandomWalkPoint());
            //Debug.Log("PatrolActionExecute new WalkPoint");
        }
    }
}
