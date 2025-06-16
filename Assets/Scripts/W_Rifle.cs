using UnityEngine;

public class W_Rifle : Weapon
{
    public override void Awake()
    {
        WeaponType = Item.WeaponType.Rifle;
        AmmoType = Item.AmmoType.Rifle;

        damage = 11f;
        fireRate = 666.7f;
        reloadTime = 3.07f;
        maxAmmo = 90;

        currentAmmo = 0;
        magMaxAmmo = 30;
        magCurrentAmmo = magMaxAmmo;

        isReloadable = true;

        spreadAngle_base = 1.0f; // Разброс пуль
        spreadAngle_moving = 1.5f;
        spreadAngle_firing = 0.3f;
        spreadAngle_max = 3f;
        attackDistance = 1000f;

        //targetLayer = LayerMask.GetMask("Target");
        //obstacleLayer = LayerMask.GetMask("Obstacle");

        weaponPrefab = Resources.Load<GameObject>("Prefabs/Rifle");

        timeSinceLastAttack = 0f;
        recoveryTime = 0.339f;
        equipTime = 1f;

        isHolderMoving = false;
        spreadAngle_current = spreadAngle_base;
    }
}
