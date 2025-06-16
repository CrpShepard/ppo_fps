using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Grenade : MonoBehaviour
{
    ITarget source;
    LayerMask targetLayer;
    Rigidbody rb;
    float baseDamage;
    float countdown;
    bool hasExploded = false;
    bool isCooked = false;
    float explosionDelay = 3f;
    float explosionRadius = 10f;

    public void SetGrenade(ITarget source, float baseDamage, float explosionRadius, bool cook)
    {
        this.source = source;
        this.baseDamage = baseDamage;
        this.explosionRadius = explosionRadius;
        this.isCooked = cook;
        targetLayer = LayerMask.GetMask("Target");

        rb.detectCollisions = true;
        rb.useGravity = true;
        rb.isKinematic = false;

        gameObject.layer = LayerMask.NameToLayer("Grenade");
    }

    [Header("Explosion Prefab")]
    public GameObject explosionEffectPrefab;
    Vector3 explosionParticleOffset = new Vector3(0, 0.05f, 0);

    //[Header("Audio Effects")]

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.detectCollisions = false;
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    private void Start()
    {
        countdown = explosionDelay;
        targetLayer = LayerMask.GetMask("Target");
    }

    private void Update()
    {
        if (!hasExploded && isCooked)
        {
            countdown -= Time.deltaTime;
            if (countdown <= 0f)
            {
                Explode();
                hasExploded = true;
            }
        }
    }

    void Explode()
    {
        GameObject explosionEffect = Instantiate(explosionEffectPrefab, transform.position + explosionParticleOffset, Quaternion.identity);

        Destroy(explosionEffect, 1.5f);

        // звук взрыва

        NearbyDamageApply();

        Destroy(gameObject, 0.5f);
    }

    void NearbyDamageApply()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider collider in colliders)
        {
            
            Vector3 direction = (collider.transform.position - transform.position).normalized;
            if (Physics.SphereCast(transform.position, 0.5f, direction, out RaycastHit hit, explosionRadius, targetLayer))
            {
                
                if (hit.transform.TryGetComponent<ITarget>(out ITarget enemy)) 
                {
                    Debug.Log("ITarget found");
                    // квадратичное затухание
                    float damage = baseDamage * (1 - Mathf.Sqrt(hit.distance / explosionRadius));

                    enemy.TakeDamage(damage, source);
                    if (source is IAgent agent) { agent._AddReward(); }
                }
            }
        }
    }
        
}
