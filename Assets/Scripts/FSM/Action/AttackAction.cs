using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "FSM/Actions/Attack")]
public class AttackAction : FSMAction
{
    public IEnumerator AttackEnemy(FSMEnemy stateMachine)
    {
        var navMeshAgent = stateMachine.navMeshAgent;
        var currentTarget = stateMachine.currentTarget;
        var transform = stateMachine.transform;
        var currentWeapon = stateMachine.currentWeapon;
        var rotationSpeed = stateMachine.rotationSpeed;

        stateMachine.isAttacking = true;
        stateMachine.navMeshAgent.updateRotation = false;
        stateMachine.navMeshAgent.angularSpeed = 0f;

        while (true)
        {
            if (currentTarget != null && Vector3.Distance(currentTarget.position, transform.position) <= currentWeapon.attackDistance && stateMachine.distanceToTarget > -1f)
            {
                Vector3 targetDirection = currentTarget.position - transform.position;
                float singleStep = rotationSpeed * Time.deltaTime;
                Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
                //stateMachine.rb.MoveRotation(Quaternion.LookRotation(newDirection));
                stateMachine.transform.rotation = Quaternion.LookRotation(newDirection);

                if (Physics.SphereCast(transform.position, currentWeapon.spreadAngle_max / 2, targetDirection, out RaycastHit hit, currentWeapon.attackDistance, LayerMask.GetMask("Target")))
                {
                    stateMachine.Attack();
                }
            }
            else { break; }

            yield return null;
        }
        stateMachine.isAttacking = false;
        stateMachine.navMeshAgent.updateRotation = true;
        stateMachine.navMeshAgent.angularSpeed = rotationSpeed;

        yield return null;
    }

    public override void Execute(FSMEnemy stateMachine)
    {
        var isAttacking = stateMachine.isAttacking;
        var currentTarget = stateMachine.currentTarget;

        if (!isAttacking && currentTarget != null)
        {
           stateMachine.attackCoroutine = stateMachine.StartCoroutine(AttackEnemy(stateMachine));
        }
    }
}
