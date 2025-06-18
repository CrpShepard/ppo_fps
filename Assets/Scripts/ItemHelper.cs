using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class ItemHelper : MonoBehaviour
{
    ITarget target;

    public void SetTarget(ITarget target)
    {
        this.target = target;
    }

    public IEnumerator ItemPoint()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            var weaponPointPistol = target.weaponPointPistol = Vector3.zero;
            var weaponPointRifle = target.weaponPointRifle = Vector3.zero;
            var weaponPointGrenade = target.weaponPointGrenade = Vector3.zero;
            var healthPackPoint = target.healthPackPoint = Vector3.zero;
            var armorPackPoint = target.armorPackPoint = Vector3.zero;
            var ammoPackPointPistol = target.ammoPackPointPistol = Vector3.zero;
            var ammoPackPointRifle = target.ammoPackPointRifle = Vector3.zero;
            var ammoPackPointGrenade = target.ammoPackPointGrenade = Vector3.zero;

            var levelEnv = target.levelEnv;
            var weapons = target.weapons;
            var transform = target.GetGameObject().transform;

            foreach (var weapon in levelEnv.weapons)
            {
                if (weaponPointPistol == Vector3.zero && !weapons.Any(w => w.WeaponType == weapon.weaponType) && weapon.weaponType == Item.WeaponType.Pistol && weapon.gameObject.activeSelf)
                {
                    //weaponPointPistol = weapon.Item1;
                    target.weaponPointPistol = weapon.transform.position;
                }
                else if (Vector3.Distance(weapon.transform.position, transform.position) < Vector3.Distance(weaponPointPistol, transform.position) && !weapons.Any(w => w.WeaponType == weapon.weaponType) && weapon.weaponType == Item.WeaponType.Pistol && weapon.gameObject.activeSelf)
                {
                    //weaponPointPistol = weapon.Item1;
                    target.weaponPointPistol = weapon.transform.position;
                }

                if (weaponPointRifle == Vector3.zero && !weapons.Any(w => w.WeaponType == weapon.weaponType) && weapon.weaponType == Item.WeaponType.Rifle && weapon.gameObject.activeSelf)
                {
                    //weaponPointRifle = weapon.Item1;
                    target.weaponPointRifle = weapon.transform.position;
                }
                else if (Vector3.Distance(weapon.transform.position, transform.position) < Vector3.Distance(weaponPointRifle, transform.position) && !weapons.Any(w => w.WeaponType == weapon.weaponType) && weapon.weaponType == Item.WeaponType.Rifle && weapon.gameObject.activeSelf)
                {
                    //weaponPointRifle = weapon.Item1;
                    target.weaponPointRifle = weapon.transform.position;
                }

                if (weaponPointGrenade == Vector3.zero && !weapons.Any(w => w.WeaponType == weapon.weaponType) && weapon.weaponType == Item.WeaponType.Grenade && weapon.gameObject.activeSelf)
                {
                    //weaponPointGrenade = weapon.Item1;
                    target.weaponPointGrenade = weapon.transform.position;
                }
                else if (Vector3.Distance(weapon.transform.position, transform.position) < Vector3.Distance(weaponPointGrenade, transform.position) && !weapons.Any(w => w.WeaponType == weapon.weaponType) && weapon.weaponType == Item.WeaponType.Grenade && weapon.gameObject.activeSelf)
                {
                    //weaponPointGrenade = weapon.Item1;
                    target.weaponPointGrenade = weapon.transform.position;
                }

                if (ammoPackPointPistol == Vector3.zero && weapons.Any(w => w.WeaponType == weapon.weaponType) && weapon.weaponType == Item.WeaponType.Pistol && weapon.gameObject.activeSelf)
                {
                    //ammoPackPointPistol = weapon.Item1;
                    target.ammoPackPointPistol = weapon.transform.position;
                }
                else if (Vector3.Distance(weapon.transform.position, transform.position) < Vector3.Distance(ammoPackPointPistol, transform.position) && weapons.Any(w => w.WeaponType == weapon.weaponType) && weapon.weaponType == Item.WeaponType.Pistol && weapon.gameObject.activeSelf)
                {
                    //ammoPackPointPistol = weapon.Item1;
                    target.ammoPackPointPistol = weapon.transform.position;
                }

                if (ammoPackPointRifle == Vector3.zero && weapons.Any(w => w.WeaponType == weapon.weaponType) && weapon.weaponType == Item.WeaponType.Rifle && weapon.gameObject.activeSelf)
                {
                    //ammoPackPointRifle = weapon.Item1;
                    target.ammoPackPointRifle = weapon.transform.position;
                }
                else if (Vector3.Distance(weapon.transform.position, transform.position) < Vector3.Distance(ammoPackPointRifle, transform.position) && weapons.Any(w => w.WeaponType == weapon.weaponType) && weapon.weaponType == Item.WeaponType.Rifle && weapon.gameObject.activeSelf)
                {
                    //ammoPackPointRifle = weapon.Item1;
                    target.ammoPackPointRifle = weapon.transform.position;
                }

                if (ammoPackPointGrenade == Vector3.zero && weapons.Any(w => w.WeaponType == weapon.weaponType) && weapon.weaponType == Item.WeaponType.Grenade && weapon.gameObject.activeSelf)
                {
                    //ammoPackPointGrenade = weapon.Item1;
                    target.ammoPackPointGrenade = weapon.transform.position;
                }
                else if (Vector3.Distance(weapon.transform.position, transform.position) < Vector3.Distance(ammoPackPointGrenade, transform.position) && weapons.Any(w => w.WeaponType == weapon.weaponType) && weapon.weaponType == Item.WeaponType.Grenade && weapon.gameObject.activeSelf)
                {
                    //ammoPackPointGrenade = weapon.Item1;
                    target.ammoPackPointGrenade = weapon.transform.position;
                }
            }

            foreach (var ammoPack in levelEnv.ammoPacks)
            {
                if (ammoPackPointPistol == Vector3.zero && weapons.Any(w => w.AmmoType == ammoPack.ammoType) && ammoPack.ammoType == Item.AmmoType.Pistol && ammoPack.gameObject.activeSelf)
                {
                    //ammoPackPointPistol = ammoPack.Item1;
                    target.ammoPackPointPistol = ammoPack.transform.position;
                }
                else if (Vector3.Distance(ammoPack.transform.position, transform.position) < Vector3.Distance(ammoPackPointPistol, transform.position) && weapons.Any(w => w.AmmoType == ammoPack.ammoType) && ammoPack.ammoType == Item.AmmoType.Pistol && ammoPack.gameObject.activeSelf)
                {
                    //ammoPackPointPistol = ammoPack.Item1;
                    target.ammoPackPointPistol = ammoPack.transform.position;
                }

                if (ammoPackPointRifle == Vector3.zero && weapons.Any(w => w.AmmoType == ammoPack.ammoType) && ammoPack.ammoType == Item.AmmoType.Pistol && ammoPack.gameObject.activeSelf)
                {
                    //ammoPackPointRifle = ammoPack.Item1;
                    target.ammoPackPointRifle = ammoPack.transform.position;
                }
                else if (Vector3.Distance(ammoPack.transform.position, transform.position) < Vector3.Distance(ammoPackPointRifle, transform.position) && weapons.Any(w => w.AmmoType == ammoPack.ammoType) && ammoPack.ammoType == Item.AmmoType.Rifle && ammoPack.gameObject.activeSelf)
                {
                    //ammoPackPointRifle = ammoPack.Item1;
                    target.ammoPackPointRifle = ammoPack.transform.position;
                }
            }

            foreach (var healthPack in levelEnv.healthPacks)
            {
                if (healthPackPoint == Vector3.zero && healthPack.gameObject.activeSelf)
                {
                    //healthPackPoint = healthPack;
                    target.healthPackPoint = healthPack.transform.position;
                }
                else if (Vector3.Distance(healthPack.transform.position, transform.position) < Vector3.Distance(healthPackPoint, transform.position) && healthPack.gameObject.activeSelf)
                {
                    //healthPackPoint = healthPack;
                    target.healthPackPoint = healthPack.transform.position;
                }
            }

            foreach (var armorPack in levelEnv.armorPacks)
            {
                if (armorPackPoint == Vector3.zero && armorPack.gameObject.activeSelf)
                {
                    //armorPackPoint = armorPack;
                    target.armorPackPoint = armorPack.transform.position;
                }
                else if (Vector3.Distance(armorPack.transform.position, transform.position) < Vector3.Distance(armorPackPoint, transform.position) && armorPack.gameObject.activeSelf)
                {
                    //armorPackPoint = armorPack;
                    target.armorPackPoint = armorPack.transform.position;
                }
            }

        }
    }

    Tuple<float, Vector3> ItemPriorityCalculate()
    {
        var transform = target.GetGameObject().transform;

        var weaponPointPistol = target.weaponPointPistol;
        var weaponPointRifle = target.weaponPointRifle;
        var weaponPointGrenade = target.weaponPointGrenade;
        var ammoPackPointPistol = target.ammoPackPointPistol;
        var ammoPackPointRifle = target.ammoPackPointRifle;
        var ammoPackPointGrenade = target.ammoPackPointGrenade;
        var healthPackPoint = target.healthPackPoint;
        var armorPackPoint = target.armorPackPoint;

        var health = target.currentHealth;
        var armor = target.currentArmor;

        var weapons = target.weapons;

        int ammoPistol = 0;
        int ammoRifle = 0;
        int ammoGrenade = 0;

        if (weapons.FirstOrDefault(w => w.WeaponType == Item.WeaponType.Pistol))
            ammoPistol = target.weapons.Where(w => w.WeaponType == Item.WeaponType.Pistol).FirstOrDefault().currentAmmo;
        //int ammoPistolMax = target.weapons.Where(w => w.WeaponType == Item.WeaponType.Pistol).FirstOrDefault().maxAmmo;
        int ammoPistolMax = 60;
        if (weapons.FirstOrDefault(w => w.WeaponType == Item.WeaponType.Rifle))
            ammoRifle = target.weapons.Where(w => w.WeaponType == Item.WeaponType.Rifle).FirstOrDefault().currentAmmo;
        //int ammoRifleMax = target.weapons.Where(w => w.WeaponType == Item.WeaponType.Rifle).FirstOrDefault().maxAmmo;
        int ammoRifleMax = 90;
        if (weapons.FirstOrDefault(w => w.WeaponType == Item.WeaponType.Grenade))
            ammoGrenade = target.weapons.Where(w => w.WeaponType == Item.WeaponType.Grenade).FirstOrDefault().magCurrentAmmo;
        //int ammoGrenadeMax = target.weapons.Where(w => w.WeaponType == Item.WeaponType.Grenade).FirstOrDefault().magMaxAmmo;
        int ammoGrenadeMax = 5;

        float weaponPistolScore = 1f - 0.6f * Mathf.Sqrt(Vector3.Distance(weaponPointPistol, transform.position) / 10f);
        if (target.weapons.Any(w => w.WeaponType == Item.WeaponType.Pistol) || weaponPointPistol == Vector3.zero) { weaponPistolScore = 0f; }

        float weaponRifleScore = 1f - 0.5f * Mathf.Sqrt(Vector3.Distance(weaponPointRifle, transform.position) / 10f);
        if (target.weapons.Any(w => w.WeaponType == Item.WeaponType.Rifle) || weaponPointRifle == Vector3.zero) { weaponRifleScore = 0f; }

        float weaponGrenadeScore = 1f - 0.7f * Mathf.Sqrt(Vector3.Distance(weaponPointGrenade, transform.position) / 10f);
        if (target.weapons.Any(w => w.WeaponType == Item.WeaponType.Grenade) || weaponPointGrenade == Vector3.zero) { weaponGrenadeScore = 0f; }

        float ammoPistolScore = 1f - Mathf.Sqrt(Vector3.Distance(ammoPackPointPistol, transform.position) / 10f) * Mathf.Clamp(Mathf.Sqrt(ammoPistol / ammoPistolMax), Mathf.Sqrt(0.15f), 1f);
        if (ammoPistol / ammoPistolMax == 1 || ammoPackPointPistol == Vector3.zero) { ammoPistolScore = 0f; }

        float ammoRifleScore = 1f - 0.95f * Mathf.Sqrt(Vector3.Distance(ammoPackPointRifle, transform.position) / 10f) * Mathf.Clamp(Mathf.Sqrt(ammoRifle / ammoRifleMax), Mathf.Sqrt(0.15f), 1f);
        if (ammoRifle / ammoRifleMax == 1 || ammoPackPointRifle == Vector3.zero) { ammoRifleScore = 0f; }

        float ammoGrenadeScore = 1f - 1.05f * Mathf.Sqrt(Vector3.Distance(ammoPackPointGrenade, transform.position) / 10f) * Mathf.Clamp(Mathf.Sqrt(ammoGrenade / ammoGrenadeMax), Mathf.Sqrt(0.15f), 1f);
        if (ammoGrenade / ammoGrenadeMax == 1 || ammoPackPointGrenade == Vector3.zero) { ammoGrenadeScore = 0f; }

        float healthScore = 1f - (health / 100f) * Mathf.Sqrt(health / 100f) * Mathf.Sqrt(Vector3.Distance(healthPackPoint, transform.position) / 10f);
        if (health == 100 || healthPackPoint == Vector3.zero) { healthScore = 0f; }

        float armorScore = 1f - (1.5f * Mathf.Sqrt(armor / 100f) * Mathf.Sqrt(Vector3.Distance(armorPackPoint, transform.position) / 10f) + 41f / health);
        if (armor == 100 || armorPackPoint == Vector3.zero) { armorScore = 0f; }

        Tuple<float, Vector3> weaponPistolPriority = Tuple.Create(weaponPistolScore, weaponPointPistol);
        Tuple<float, Vector3> weaponRiflePriority = Tuple.Create(weaponRifleScore, weaponPointRifle);
        Tuple<float, Vector3> weaponGrenadePriority = Tuple.Create(weaponGrenadeScore, weaponPointGrenade);

        Tuple<float, Vector3> ammoPistolPriority = Tuple.Create(ammoPistolScore, ammoPackPointPistol);
        Tuple<float, Vector3> ammoRiflePriority = Tuple.Create(ammoRifleScore, ammoPackPointRifle);
        Tuple<float, Vector3> ammoGrenadePriority = Tuple.Create(ammoGrenadeScore, ammoPackPointGrenade);

        Tuple<float, Vector3> healthPackPriority = Tuple.Create(healthScore, healthPackPoint);
        Tuple<float, Vector3> armorPriority = Tuple.Create(armorScore, armorPackPoint);

        // возвращается точка с предметом, у которой наибольший приоритет
        return new List<Tuple<float, Vector3>>()
        { weaponPistolPriority, weaponRiflePriority, weaponGrenadePriority,
            ammoPistolPriority, ammoRiflePriority, ammoGrenadePriority,
            healthPackPriority, armorPriority }.OrderByDescending(x => x.Item1).First();
    }

    public Vector3 ItemPriorityPoint()
    {
        // возвращается точка с предметом, у которой наибольший приоритет
        return ItemPriorityCalculate().Item2;
    }

    public float ItemPriority()
    {
        // возвращается точка с предметом, у которой наибольший приоритет
        return ItemPriorityCalculate().Item1;
    }

    public bool GetPath(NavMeshPath path, Vector3 fromPos, Vector3 toPos, int passableMask)
    {
        path.ClearCorners();

        if (NavMesh.CalculatePath(fromPos, toPos, passableMask, path) == false)
            return false;

        return true;
    }

    public float GetPathLength(NavMeshPath path, Vector3 fromPos, Vector3 toPos, int passableMask)
    {
        float lng = -1f;
        if (GetPath(path, fromPos, toPos, passableMask))
        {
            if (path.status != NavMeshPathStatus.PathInvalid)
            {
                lng = 0f;
                for (int i = 1; i < path.corners.Length; ++i)
                {
                    lng += Vector3.Distance(path.corners[i - 1], path.corners[i]);
                }
            }
        }

        return lng;
    }

}
