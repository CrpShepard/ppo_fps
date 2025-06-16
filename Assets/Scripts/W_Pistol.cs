using UnityEngine;

public class W_Pistol : Weapon
{
    public override void Awake()
    {
        WeaponType = Item.WeaponType.Pistol;
        AmmoType = Item.AmmoType.Pistol;

        damage = 8f;
        fireRate = 440f;
        reloadTime = 2.2f;

        maxAmmo = 60;
        currentAmmo = 0;
        magMaxAmmo = 18;
        magCurrentAmmo = magMaxAmmo;

        isReloadable = true;

        // OLD
        spreadAngle_base = 1.5f; // Разброс пуль
        spreadAngle_moving = 2.25f;
        spreadAngle_firing = 0.5f;
        spreadAngle_max = 5f;

        attackDistance = 1000f;

        //targetLayer = LayerMask.GetMask("Target");
        //obstacleLayer = LayerMask.GetMask("Obstacle");

        weaponPrefab = Resources.Load<GameObject>("Prefabs/Pistol");

        timeSinceLastAttack = 0f;
        recoveryTime = 0.4f;
        equipTime = 0.8f;

        isHolderMoving = false;
        spreadAngle_current = spreadAngle_base;
    }
}
