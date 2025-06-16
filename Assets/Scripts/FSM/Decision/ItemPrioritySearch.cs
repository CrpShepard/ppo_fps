using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Decisions/ItemPrioritySearch")]
public class ItemPrioritySearch : Decision
{
    public override bool Decide(FSMEnemy stateMachine)
    {
        var itemPriority = stateMachine.itemHelper.ItemPriority();
        var itemPickupDelay = stateMachine.itemPickupDelay;

        return (itemPriority >= 0.4f && !itemPickupDelay) ? true : false;
    }
}
