using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeaponHelper : MonoBehaviour
{
    ITarget target;

    public void ChangeWeapon(int index)
    {
        Weapon neededWeapon = null;
        var currentWeapon = target.currentWeapon;

        switch (index)
        {
            case 1:
                neededWeapon = target.weapons.OfType<W_Crowbar>().FirstOrDefault();
                if (neededWeapon && target.currentWeapon != neededWeapon)
                {
                    target.currentWeapon = neededWeapon;
                    target.currentWeapon.Equip(target);

                    if (target is IAgent agent)
                    {
                        if (currentWeapon && currentWeapon.magCurrentAmmo <= 0f || (agent.distanceToTarget > -1f && agent.distanceToTarget < 10f)) agent._AddReward(0.05f);
                    }
                }
                break;
            case 2:
                neededWeapon = target.weapons.OfType<W_Pistol>().FirstOrDefault();
                if (neededWeapon && target.currentWeapon != neededWeapon)
                {
                    target.currentWeapon = neededWeapon;
                    target.currentWeapon.Equip(target);

                    if (target is IAgent agent)
                    {
                        if (currentWeapon.magCurrentAmmo <= 0f || agent.distanceToTarget > 10f) agent._AddReward(0.05f);
                    }
                }
                break;
            case 3:
                neededWeapon = target.weapons.OfType<W_Rifle>().FirstOrDefault();
                if (neededWeapon && target.currentWeapon != neededWeapon)
                {
                    target.currentWeapon = neededWeapon;
                    target.currentWeapon.Equip(target);

                    if (target is IAgent agent)
                    {
                        if (currentWeapon.magCurrentAmmo <= 0f || agent.distanceToTarget > 10f) agent._AddReward(0.05f);
                    }
                }
                break;
            case 4:
                neededWeapon = target.weapons.OfType<W_Grenade>().FirstOrDefault();
                if (neededWeapon && target.currentWeapon != neededWeapon && neededWeapon.magCurrentAmmo > 0)
                {
                    target.currentWeapon = neededWeapon;
                    target.currentWeapon.Equip(target);

                    if (target is IAgent agent)
                    {
                        if (currentWeapon.magCurrentAmmo <= 0f || agent.distanceToTarget > 10f) agent._AddReward(0.05f);
                    }
                }
                break;
        }
    }

    public void ChangeWeapon(Weapon weapon)
    {
        Weapon neededWeapon = null;

        switch (weapon)
        {
            case W_Crowbar:
                neededWeapon = target.weapons.OfType<W_Crowbar>().FirstOrDefault();
                if (neededWeapon && target.currentWeapon != neededWeapon)
                {
                    target.currentWeapon = neededWeapon;
                    target.currentWeapon.Equip(target);
                }
                break;
            case W_Pistol:
                neededWeapon = target.weapons.OfType<W_Pistol>().FirstOrDefault();
                if (neededWeapon && target.currentWeapon != neededWeapon)
                {
                    target.currentWeapon = neededWeapon;
                    target.currentWeapon.Equip(target);
                }
                break;
            case W_Rifle:
                neededWeapon = target.weapons.OfType<W_Rifle>().FirstOrDefault();
                if (neededWeapon && target.currentWeapon != neededWeapon)
                {
                    target.currentWeapon = neededWeapon;
                    target.currentWeapon.Equip(target);
                }
                break;
            case W_Grenade:
                neededWeapon = target.weapons.OfType<W_Grenade>().FirstOrDefault();
                if (neededWeapon && target.currentWeapon != neededWeapon && neededWeapon.magCurrentAmmo > 0)
                {
                    target.currentWeapon = neededWeapon;
                    target.currentWeapon.Equip(target);
                }
                break;
        }
    }

    public void Attack(Vector3 position, Vector3 forward)
    {
        if (target.currentWeapon != null) { target.currentWeapon.Attack(target, position, forward); }
    }

    public void Reload()
    {
        if (target.currentWeapon.isReloadable) 
        {
            var currentWeapon = target.currentWeapon;
            if (currentWeapon.magCurrentAmmo / currentWeapon.magMaxAmmo < 0.5f && currentWeapon.currentAmmo > 0 && target is IAgent agent)
            {
                agent._AddReward(0.05f);
            } 

            target.currentWeapon.Reload();
             
        }
    }

    public bool AddWeaponBool(Weapon weapon)
    {
        bool result = false;
        bool alreadyExists = target.weapons.Any(w => w.GetType() == weapon.GetType());

        if (alreadyExists) // само оружие уже есть, но можно подобрать в виде патронов
        {
            Weapon neededWeapon;
            if (weapon is W_Pistol pistol)
            {
                neededWeapon = target.weapons.OfType<W_Pistol>().FirstOrDefault();
                if (neededWeapon != null && neededWeapon.CheckAmmo()) { result = true; neededWeapon.AddAmmo(); }
            }
            else if (weapon is W_Rifle rifle)
            {
                neededWeapon = target.weapons.OfType<W_Rifle>().FirstOrDefault();
                if (neededWeapon != null && neededWeapon.CheckAmmo()) { result = true; neededWeapon.AddAmmo(); }
            }
            else if (weapon is W_Grenade grenade)
            {
                neededWeapon = target.weapons.OfType<W_Grenade>().FirstOrDefault();
                if (neededWeapon != null && neededWeapon.CheckAmmo()) { result = true; neededWeapon.AddAmmo(); }
            }
        }
        else if (!alreadyExists) { AddWeapon(weapon); result = true; }

        return result;
    }

    public void AddWeapon(Weapon weapon)
    {
        bool alreadyExists = target.weapons.Any(w => w.GetType() == weapon.GetType());

        if (!alreadyExists)
        {
            target.weapons.Add(weapon);

            if (target is IAgent agent) { agent._AddReward(0.01f); }
        }
    }

    public bool AddAmmo(Item.AmmoType ammoType)
    {
        bool result = false;
        Weapon neededWeapon;
        switch (ammoType)
        {
            case Item.AmmoType.Pistol:
                neededWeapon = target.weapons.OfType<W_Pistol>().FirstOrDefault();
                if (neededWeapon != null && neededWeapon.CheckAmmo()) { result = true; neededWeapon.AddAmmo(); }
                break;
            case Item.AmmoType.Rifle:
                neededWeapon = target.weapons.OfType<W_Rifle>().FirstOrDefault();
                if (neededWeapon != null && neededWeapon.CheckAmmo()) { result = true; neededWeapon.AddAmmo(); }
                break;
        }

        return result;
    }

    public GameObject EquipWeaponPrefab(GameObject prefab)
    {
        foreach (Transform child in target.weaponHolder.transform)
        {
            Destroy(child.gameObject);
        }

        Quaternion originalRotation = prefab.transform.rotation;
        Quaternion yRotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        Quaternion yRotation2 = Quaternion.Euler(0, -2.08f, 0);
        Quaternion finalRotation = yRotation * originalRotation * yRotation2;

        GameObject weaponObj = Instantiate(prefab, target.weaponHolder.position, finalRotation);
        weaponObj.transform.parent = target.weaponHolder.transform;

        return weaponObj;
    }

    // Init
    public void SetInterface(ITarget target) { this.target = target; }
}
