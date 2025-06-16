using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Decisions/LowHealth")]
public class LowHealth : Decision
{
    public override bool Decide(FSMEnemy stateMachine)
    {
        var health = stateMachine.currentHealth;

        return (health < 60f) ? true : false;
    }
}
