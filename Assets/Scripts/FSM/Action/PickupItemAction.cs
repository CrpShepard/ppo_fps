using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Actions/PickupItem")]
public class PickupItemAction : FSMAction
{
    public override void Execute(FSMEnemy stateMachine)
    {
        stateMachine.isPatrolling = false;
        stateMachine.isSearching = false;
        stateMachine.itemPickuped = false;
        stateMachine.StopHide();
        stateMachine.timeHide = 0f;
        stateMachine.timeSearch = 0f;

        var navMeshAgent = stateMachine.navMeshAgent;

        if (!stateMachine.isPickingUpItem)
        {
            stateMachine.isPickingUpItem = true;
            navMeshAgent.SetDestination(stateMachine.itemHelper.ItemPriorityPoint());
        }
        else if (stateMachine.IsWalkPointReached())
        {
            stateMachine.itemPickupDelay = true;
            stateMachine.itemPickuped = true;
        }
    }
}
