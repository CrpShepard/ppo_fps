using UnityEngine;

public class W_Crowbar : Weapon
{
    public override void Attack(out ITarget hitTarget, Vector3 pos, Vector3 dir)
    {
        hitTarget = null;
        if (!CanAttack()) return;

        // Логика выстрела (Raycast)
        Debug.DrawRay(pos, dir * attackDistance, Color.magenta, 5f);
        //Debug.Log("Crowbar Attack");
        if (Physics.Raycast(pos, dir, out RaycastHit hit, attackDistance, targetLayer))
        {
            Debug.Log("Raycast hit the enemy");
            if (hit.transform.TryGetComponent<ITarget>(out ITarget enemy)) { hitTarget = enemy; }
        }

        timeSinceLastAttack = 0f;
    }

    private void Awake()
    {
        damage = 25f;
        fireRate = 150f;
        attackDistance = 3.25f;

        currentAmmo = 1;
        maxAmmo = 1;
        magMaxAmmo = 1;
        magCurrentAmmo = 1;

        targetLayer = LayerMask.GetMask("Target");

        timeSinceLastAttack = 0f;
    }
}
