using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Decisions/ItemPriorityPatrol")]
public class ItemPriorityPatrol : Decision
{
    public override bool Decide(FSMEnemy stateMachine)
    {
        var itemPriority = stateMachine.itemHelper.ItemPriority();
        var itemPickupDelay = stateMachine.itemPickupDelay;

        return (itemPriority >= 0.2f && !itemPickupDelay) ? true : false;
    }
}
