using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Decisions/ItemPriorityLow")]
public class ItemPriorityLow : Decision
{
    public override bool Decide(FSMEnemy stateMachine)
    {
        var itemPriority = stateMachine.itemHelper.ItemPriority();

        return (itemPriority < 0.1f) ? true : false;
    }
}
