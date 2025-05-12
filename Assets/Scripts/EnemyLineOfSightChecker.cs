using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class EnemyLineOfSightChecker : MonoBehaviour
{
    public SphereCollider Collider;
    public float FieldOfView = 100f;
    public LayerMask LineOfSightLayers;

    //public delegate void GainSightEvent(Transform Target);
    public delegate void GainSightEvent(LastKnownTargetTransform Target);
    public GainSightEvent OnGainSight;
    //public delegate void LoseSightEvent(Transform Target);
    public delegate void LoseSightEvent(LastKnownTargetTransform Target);
    public LoseSightEvent OnLoseSight;

    private Coroutine CheckForLineOfSightCoroutine;

    private void Awake()
    {
        Collider = GetComponent<SphereCollider>();
    }

    //private void OnTriggerEnter(Collider other)
    public void StartCheckForLineOfSightCoroutine(LastKnownTargetTransform other)
    {
        //if (!CheckLineOfSight(other.transform))
        if (!CheckLineOfSight(other))
        {
            CheckForLineOfSightCoroutine = StartCoroutine(CheckForLineOfSight(other));
        }
    }

    //private void OnTriggerExit(Collider other)
    public void EndCheckForLineOfSightCoroutine(LastKnownTargetTransform other)
    {
        //OnLoseSight?.Invoke(other.transform);
        OnLoseSight?.Invoke(other);
        if (CheckForLineOfSightCoroutine != null)
        {
            StopCoroutine(CheckForLineOfSightCoroutine);
        }
    }

    //private bool CheckLineOfSight(Transform Target)
    private bool CheckLineOfSight(LastKnownTargetTransform Target)
    {
        // TODO добавить угол обзора со стороны врага, чтобы можно было считать что враг не видит спиной вперед к агенту
        //Vector3 direction = (Target.transform.position - transform.position).normalized;
        Vector3 direction = (Target.position - transform.position).normalized;
        float dotProduct = Vector3.Dot(transform.forward, direction);
        if (dotProduct >= Mathf.Cos(FieldOfView))
        {
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, Collider.radius, LineOfSightLayers))
            {
                OnGainSight?.Invoke(Target);
                return true;
            }
        }

        return false;
    }

    //private IEnumerator CheckForLineOfSight(Transform Target)
    private IEnumerator CheckForLineOfSight(LastKnownTargetTransform Target)
    {
        WaitForSeconds Wait = new WaitForSeconds(0.5f);

        while (!CheckLineOfSight(Target))
        {
            yield return Wait;
        }
    }
}