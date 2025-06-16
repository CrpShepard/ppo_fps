using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Decisions/ItemPickedup")]
public class ItemPickedup : Decision
{
    public override bool Decide(FSMEnemy stateMachine)
    {
        var itemPickedup = stateMachine.itemPickuped;

        return itemPickedup;
    }
}
