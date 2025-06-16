using Unity.MLAgents;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "FSM/Actions/EvadeGrenade")]
public class EvadeGrenadeAction : FSMAction
{
    public Vector3 EvadeDirection(FSMEnemy stateMachine)
    {
        var grenades = stateMachine.fieldOfView.visibleGrenades;
        var transform = stateMachine.transform;

        Vector3 totalThreatDirection = Vector3.zero;

        if (grenades != null)
        foreach (var grenade in grenades)
        {
            Vector3 dirToGrenade = transform.position - grenade.transform.position;
            // „ем ближе граната, тем сильнее еЄ вли€ние на направление уклонени€
            float distanceFactor = 1f / Mathf.Max(0.1f, dirToGrenade.magnitude);
            totalThreatDirection += dirToGrenade.normalized * distanceFactor;
        }

        return totalThreatDirection.normalized;
    }

    bool IsPointSafe(FSMEnemy stateMachine, Vector3 point)
    {
        var grenades = stateMachine.fieldOfView.visibleGrenades;

        if (grenades != null)
        foreach (var grenade in grenades)
        {
            if (Vector3.Distance(point, grenade.transform.position) < 10.1f)
                return false;
        }
        return true;
    }

    void TryFindAlternativeEscape(FSMEnemy stateMachine)
    {
        var transform = stateMachine.transform;
        var grenades = stateMachine.fieldOfView.visibleGrenades;
        var navMeshAgent = stateMachine.navMeshAgent;

        Vector3 evadeDirection = EvadeDirection(stateMachine);

        // ѕробуем несколько точек вдоль направлени€, увеличива€ дистанцию
        for (float distanceMultiplier = 1.5f; distanceMultiplier <= 3f; distanceMultiplier += 0.5f)
        {
            Vector3 testPoint = transform.position + evadeDirection * (13f * distanceMultiplier);

            if (NavMesh.SamplePosition(testPoint, out NavMeshHit hit, 13f * 2f, navMeshAgent.areaMask))
            {
                if (IsPointSafe(stateMachine, hit.position))
                {
                    navMeshAgent.SetDestination(hit.position);
                    return;
                }
            }
        }

        // ≈сли ничего не найдено, просто бежим в противоположную сторону от ближайшей гранаты
        Vector3 fallbackDirection = (transform.position - grenades[0].transform.position).normalized;
        Vector3 fallbackPoint = transform.position + fallbackDirection * 13f;

        if (NavMesh.SamplePosition(fallbackPoint, out NavMeshHit fallbackHit, 13f, navMeshAgent.areaMask))
        {
            navMeshAgent.SetDestination(fallbackHit.position);
        }
    }

    public void EvadePoint(FSMEnemy stateMachine)
    {
        var transform = stateMachine.transform;
        var grenades = stateMachine.fieldOfView.visibleGrenades;
        var navMeshAgent = stateMachine.navMeshAgent;

        Vector3 evadeDirection = EvadeDirection(stateMachine);
        Vector3 evadePoint = transform.position + evadeDirection * 13f;

        if (NavMesh.SamplePosition(evadePoint, out NavMeshHit hit, 13f, navMeshAgent.areaMask))
        {
            
            if (IsPointSafe(stateMachine, hit.position))
            {
                navMeshAgent.SetDestination(hit.position);
            }
            else
            {
                TryFindAlternativeEscape(stateMachine);
            }
        }
        else { TryFindAlternativeEscape(stateMachine); }
    }

    public override void Execute(FSMEnemy stateMachine)
    {
        // FIX ME
        stateMachine.isPatrolling = false;
        stateMachine.isSearching = false;
        stateMachine.StopHide();
        stateMachine.takenDamage = false;
        stateMachine.takenDamageTimer = 0f;
        stateMachine.isPickingUpItem = false;
        stateMachine.itemPickuped = false;
        stateMachine.timeHide = 0f;
        stateMachine.timeSearch = 0f;

        EvadePoint(stateMachine);
    }
}
