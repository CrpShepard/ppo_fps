using UnityEngine;

public class W_Crowbar : Weapon
{
    public override void Attack(ITarget source, Vector3 position, Vector3 dir)
    {
        if (!CanAttack()) return;

        // Логика выстрела (Raycast)
        Debug.DrawRay(position, dir * attackDistance, Color.magenta, 2f);
        //Debug.Log("Crowbar Attack");
        if (Physics.Raycast(position, dir, out RaycastHit hit, attackDistance))
        {
            //Debug.Log("Crowbar hits enemy");
            if (hit.transform.TryGetComponent<ITarget>(out ITarget enemy)) 
            {
                enemy.TakeDamage(damage, source);
                if (source is IAgent agent) { agent._AddReward(0.1f); }
            }

            Instantiate(ImpactParticle, hit.point, Quaternion.LookRotation(hit.normal));
        }

        // TODO hitEffect


        timeSinceLastAttack = 0f;
    }

    public override void Awake()
    {
        damage = 25f;
        fireRate = 150f;
        attackDistance = 3.25f;

        currentAmmo = 1;
        maxAmmo = 1;
        magMaxAmmo = 1;
        magCurrentAmmo = 1;

        isReloadable = false;

        //targetLayer = LayerMask.GetMask("Target");
        //obstacleLayer = LayerMask.GetMask("Obstacle");

        weaponPrefab = Resources.Load<GameObject>("Prefabs/Crowbar");

        timeSinceLastAttack = 0f;
        equipTime = 0.7f;
    }
}
