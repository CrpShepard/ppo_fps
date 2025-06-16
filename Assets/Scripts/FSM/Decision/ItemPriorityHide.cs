using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Decisions/ItemPriorityHide")]
public class ItemPriorityHide : Decision
{
    public override bool Decide(FSMEnemy stateMachine)
    {
        var itemPriority = stateMachine.itemHelper.ItemPriority();
        var itemPickupDelay = stateMachine.itemPickupDelay;
        
        return (itemPriority >= 0.7f && !itemPickupDelay && !stateMachine.currentTarget) ? true : false;
    }
}
