using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Actions/Search")]
public class SearchAction : FSMAction
{
    public override void Execute(FSMEnemy stateMachine)
    {
        var navMeshAgent = stateMachine.navMeshAgent;
        var lastTargetTransform = stateMachine.lastKnownTargetTransform;

        stateMachine.StopAttackEnemy();
        stateMachine.isPatrolling = false;
        stateMachine.StopHide();
        stateMachine.isPickingUpItem = false;
        stateMachine.itemPickuped = false;
        stateMachine.timeHide = 0f;

        if (!stateMachine.isSearching)
        {
            stateMachine.timeSearch = 0;
            stateMachine.isSearching = true;
            navMeshAgent.SetDestination(lastTargetTransform.position);
        }
        else
        {
            if (stateMachine.IsWalkPointReached())
            {
                navMeshAgent.SetDestination(stateMachine.RandomSearchPoint());
            }
        }
    }
}
