using UnityEngine;
using UnityEngine.AI;
using static Cinemachine.CinemachineTriggerAction.ActionSettings;

[CreateAssetMenu(menuName = "FSM/Actions/Chase")]
public class ChaseAction : FSMAction
{
    public override void Execute(FSMEnemy stateMachine)
    {
        var navMeshAgent = stateMachine.navMeshAgent;
        var currentTarget = stateMachine.currentTarget;

        stateMachine.isPatrolling = false;
        stateMachine.isSearching = false;
        stateMachine.StopHide();
        stateMachine.takenDamage = false;
        stateMachine.takenDamageTimer = 0f;
        stateMachine.isPickingUpItem = false;
        stateMachine.itemPickuped = false;
        stateMachine.timeHide = 0f;
        stateMachine.timeSearch = 0f;

        if (currentTarget != null && !stateMachine.isAttacking)
        {
            navMeshAgent.SetDestination(currentTarget.position);
        }
    }
}
