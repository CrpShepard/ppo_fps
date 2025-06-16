using UnityEngine;
using static Cinemachine.CinemachineTriggerAction.ActionSettings;
using UnityEngine.AI;
using System.Collections;

[CreateAssetMenu(menuName = "FSM/Actions/Hide")]
public class HideAction : FSMAction
{
    Transform transform;

    public IEnumerator Hide(FSMEnemy stateMachine)
    {
        var currentTarget = stateMachine.currentTarget;
        var Colliders = stateMachine.Colliders;
        var lastKnownTargetTransform = stateMachine.lastKnownTargetTransform;
        transform = stateMachine.transform;
        var navMeshAgent = stateMachine.navMeshAgent;

        stateMachine.isHiding = true;
        stateMachine.timeHide = 0;
        float HideSensitivity = -0.55f;
        float MinPlayerDistance = 5f;

        WaitForSeconds Wait = new WaitForSeconds(0.25f);
        while (true)
        {
            Vector3 targetPos;

            if (currentTarget != null)
            {
                targetPos = currentTarget.transform.position;
            }
            else { targetPos = lastKnownTargetTransform.position; }

            for (int i = 0; i < Colliders.Length; i++)
            {
                Colliders[i] = null;
            }

            int hits = Physics.OverlapSphereNonAlloc(transform.position, 20f, Colliders, LayerMask.GetMask("Obstacle"));

            int hitReduction = 0;
            for (int i = 0; i < hits; i++)
            {
                if (Vector3.Distance(Colliders[i].transform.position, targetPos) < MinPlayerDistance || Colliders[i].bounds.size.y < 2f)
                {
                    Colliders[i] = null;
                    hitReduction++;
                }
            }
            hits -= hitReduction;

            System.Array.Sort(Colliders, ColliderArraySortComparer);

            for (int i = 0; i < hits; i++)
            {
                if (NavMesh.SamplePosition(Colliders[i].transform.position, out NavMeshHit hit, 2f, navMeshAgent.areaMask))
                {
                    if (!NavMesh.FindClosestEdge(hit.position, out hit, navMeshAgent.areaMask))
                    {
                        //Debug.LogError($"Unable to find edge close to {hit.position}");
                        navMeshAgent.SetDestination(stateMachine.RandomWalkPoint());
                    }

                    if (Vector3.Dot(hit.normal, (targetPos - hit.position).normalized) < HideSensitivity)
                    {
                        navMeshAgent.SetDestination(hit.position);
                        break;
                    }
                    else
                    {
                        // Since the previous spot wasn't facing "away" enough from teh target, we'll try on the other side of the object
                        if (NavMesh.SamplePosition(Colliders[i].transform.position - (targetPos - hit.position).normalized * 2, out NavMeshHit hit2, 2f, navMeshAgent.areaMask))
                        {
                            if (!NavMesh.FindClosestEdge(hit2.position, out hit2, navMeshAgent.areaMask))
                            {
                                //Debug.LogError($"Unable to find edge close to {hit2.position} (second attempt)");
                                navMeshAgent.SetDestination(stateMachine.RandomWalkPoint());
                            }

                            if (Vector3.Dot(hit2.normal, (targetPos - hit2.position).normalized) < HideSensitivity)
                            {
                                navMeshAgent.SetDestination(hit2.position);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    //Debug.LogError($"Unable to find NavMesh near object {Colliders[i].name} at {Colliders[i].transform.position}");
                    navMeshAgent.SetDestination(stateMachine.RandomWalkPoint());
                }
            }
            yield return Wait;
        }
    }

    public int ColliderArraySortComparer(Collider A, Collider B)
    {
        if (A == null && B != null)
        {
            return 1;
        }
        else if (A != null && B == null)
        {
            return -1;
        }
        else if (A == null && B == null)
        {
            return 0;
        }
        else
        {
            return Vector3.Distance(transform.position, A.transform.position).CompareTo(Vector3.Distance(transform.position, B.transform.position));
        }
    }

    public override void Execute(FSMEnemy stateMachine)
    {
        stateMachine.StopAttackEnemy();
        stateMachine.isPatrolling = false;
        stateMachine.isSearching = false;
        stateMachine.takenDamage = false;
        stateMachine.takenDamageTimer = 0f;
        stateMachine.isPickingUpItem = false;
        stateMachine.itemPickuped = false;
        stateMachine.timeSearch = 0f;

        if (!stateMachine.isHiding)
        {
            //stateMachine.StartCoroutine(stateMachine.Hide());

            stateMachine.hidingCoroutine = stateMachine.StartCoroutine(Hide(stateMachine));
        }

        if (stateMachine.currentTarget) { stateMachine.timeHide = 0; }
    }
}
