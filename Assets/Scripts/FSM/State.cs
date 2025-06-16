using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FSM/State")]
public sealed class State : BaseState
{
    public State stateType;
    public List<FSMAction> Action = new List<FSMAction>();
    public List<Transition> Transitions = new List<Transition>();

    public override void Execute(FSMEnemy machine)
    {
        foreach (var action in Action)
            action.Execute(machine);

        foreach (var transition in Transitions)
            transition.Execute(machine);
    }
}
