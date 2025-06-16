using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class W_Grenade : Weapon
{
    public float explosionRadius = 10f;
    float throwForce = 12f;

    //Transform throwPosition;
    Vector3 throwDirection = new Vector3(0, 1, 0);

    GameObject grenadePrefab;

    public override void Attack(ITarget source, Vector3 position, Vector3 dir)
    {
        if (!CanAttack()) return;

        magCurrentAmmo--;

        // создание projectile
        ThrowGrenade(source, position, dir);

        timeSinceLastAttack = 0f;

        if (magCurrentAmmo == 0)
        {
            source.ChangeWeapon(1);
        }
    }

    void ThrowGrenade(ITarget source, Vector3 pos, Vector3 dir)
    {
        //Vector3 spawnPosition = throwPosition.position + dir;
        Vector3 spawnPosition = pos + dir;
        Quaternion rotation = Quaternion.LookRotation(dir, Vector3.up);

        GameObject grenadeObj = Instantiate(grenadePrefab, spawnPosition, rotation);
        Rigidbody rb = grenadeObj.GetComponent<Rigidbody>();

        Grenade grenade = grenadeObj.GetComponent<Grenade>();
        grenade.SetGrenade(source, damage, explosionRadius, true);

        Vector3 finalThrowDirection = (dir + throwDirection).normalized;
        rb.AddForce(finalThrowDirection * throwForce, ForceMode.VelocityChange);

        // звук броска гранаты
    }

    public override bool CheckAmmo() { return (magCurrentAmmo < magMaxAmmo) ? true : false; }

    public override void AddAmmo()
    {
        magCurrentAmmo += 1;
        Mathf.Clamp(magCurrentAmmo, 0, magMaxAmmo);
    }

    public override void Awake()
    {
        WeaponType = Item.WeaponType.Grenade;

        damage = 150f;
        fireRate = 60f;
        attackDistance = 13f;
        
        magMaxAmmo = 5;
        magCurrentAmmo = 1;

        isReloadable = false;

        //targetLayer = LayerMask.GetMask("Target");

        weaponPrefab = Resources.Load<GameObject>("Prefabs/Grenade");
        grenadePrefab = Resources.Load<GameObject>("Prefabs/Grenade");

        timeSinceLastAttack = 0f;
        equipTime = 0.75f;
    }
}
