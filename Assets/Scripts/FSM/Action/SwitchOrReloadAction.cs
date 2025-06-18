using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Actions/SwitchOrReload")]
public class SwitchOrReloadAction : FSMAction
{
    public override void Execute(FSMEnemy stateMachine)
    {
        var currentState = stateMachine.CurrentState;

        var weapons = stateMachine.weapons;
        var currentWeapon = stateMachine.currentWeapon;

        var changeWeaponDelay = stateMachine.changeWeaponDelay;

        IEnumerable<Weapon> otherWeapon;
        W_Rifle weaponRifle;
        W_Pistol weaponPistol;
        W_Grenade weaponGrenade;
        W_Crowbar weaponCrowbar;

        if (currentState is State state)
        {
            switch(state.stateType)
            {
                // перезарядка текущего оружия и перезарядка иного оружия со сменой на него
                case BaseState.State.Patrol:
                    bool changedWeaponForReload = false;

                    if (currentWeapon && currentWeapon.isReloadable && currentWeapon.currentAmmo > 0 && currentWeapon.magCurrentAmmo < currentWeapon.magMaxAmmo )
                    {
                        stateMachine.Reload();
                        break;
                    }
                     
                    foreach (var weapon in weapons.Where(weapon => weapon.isReloadable && weapon != currentWeapon))
                    {
                        if (weapon.isReloadable && weapon.currentAmmo > 0 && weapon.magCurrentAmmo < weapon.magMaxAmmo && !changeWeaponDelay)
                        {
                            stateMachine.ChangeWeapon(weapon);
                            changedWeaponForReload = true;
                            break;
                        }
                    }
                    
                    if (!changedWeaponForReload)
                    {
                        otherWeapon = weapons.Where(weapon => weapon != currentWeapon);

                        weaponRifle = otherWeapon.OfType<W_Rifle>().FirstOrDefault();
                        if (currentWeapon && weaponRifle != null && 
                            ((weaponRifle.magCurrentAmmo / weaponRifle.magMaxAmmo >= 0.5f 
                            && currentWeapon is W_Pistol && currentWeapon.magCurrentAmmo / currentWeapon.magMaxAmmo <= 0.66f ) 
                            || (weaponRifle.magCurrentAmmo / weaponRifle.magMaxAmmo > 0f && (currentWeapon is W_Crowbar || currentWeapon is W_Grenade))) && !changeWeaponDelay)
                        {
                            stateMachine.ChangeWeapon(weaponRifle);
                            break;
                        }

                        weaponPistol = otherWeapon.OfType<W_Pistol>().FirstOrDefault();
                        if (currentWeapon && weaponPistol != null &&
                            ((weaponPistol.magCurrentAmmo / weaponPistol.magMaxAmmo > 0.66f
                            && currentWeapon is W_Rifle && currentWeapon.magCurrentAmmo / currentWeapon.magMaxAmmo <= 0.2f)
                            || (weaponPistol.magCurrentAmmo / weaponPistol.magMaxAmmo > 0f && (currentWeapon is W_Crowbar || currentWeapon is W_Grenade))) && !changeWeaponDelay)
                        {
                            stateMachine.ChangeWeapon(weaponPistol);
                            break;
                        }

                        weaponGrenade = otherWeapon.OfType<W_Grenade>().FirstOrDefault();
                        if (currentWeapon && weaponGrenade != null &&
                            (weaponGrenade.magCurrentAmmo > 0
                            && (currentWeapon is W_Crowbar || currentWeapon.magCurrentAmmo <= 0)) && !changeWeaponDelay)
                        {
                            stateMachine.ChangeWeapon(weaponGrenade);
                            break;
                        }

                        weaponCrowbar = otherWeapon.OfType<W_Crowbar>().FirstOrDefault();
                        if (currentWeapon && currentWeapon.magCurrentAmmo <= 0 && !changeWeaponDelay) // до сюда доходит, если вообще не осталось патронов или гранат
                        {
                            stateMachine.ChangeWeapon(weaponCrowbar);
                            break;
                        }

                    }
                    break;
                case BaseState.State.Search:
                    otherWeapon = weapons.Where(weapon => weapon != currentWeapon);

                    weaponRifle = otherWeapon.OfType<W_Rifle>().FirstOrDefault();
                    if (currentWeapon && weaponRifle != null &&
                        ((weaponRifle.magCurrentAmmo / weaponRifle.magMaxAmmo >= 0.2f
                        && currentWeapon is W_Pistol && currentWeapon.magCurrentAmmo / currentWeapon.magMaxAmmo <= 0.2f)
                        || (weaponRifle.magCurrentAmmo / weaponRifle.magMaxAmmo > 0f && (currentWeapon is W_Crowbar || currentWeapon is W_Grenade))) && !changeWeaponDelay)
                    {
                        stateMachine.ChangeWeapon(weaponRifle);
                        break;
                    }

                    if (currentWeapon && currentWeapon.isReloadable && currentWeapon.currentAmmo > 0 && currentWeapon.magCurrentAmmo / currentWeapon.magMaxAmmo <= 0.2f)
                    {
                        stateMachine.Reload();
                        break;
                    }

                    weaponPistol = otherWeapon.OfType<W_Pistol>().FirstOrDefault();
                    if (currentWeapon && weaponPistol != null &&
                        ((weaponPistol.magCurrentAmmo / weaponPistol.magMaxAmmo >= 0.4f
                        && currentWeapon is W_Rifle && currentWeapon.magCurrentAmmo / currentWeapon.magMaxAmmo < 0.2f)
                        || (weaponPistol.magCurrentAmmo / weaponPistol.magMaxAmmo > 0f && (currentWeapon is W_Crowbar || currentWeapon is W_Grenade))) && !changeWeaponDelay)
                    {
                        stateMachine.ChangeWeapon(weaponPistol);
                        break;
                    }

                    weaponGrenade = otherWeapon.OfType<W_Grenade>().FirstOrDefault();
                    if (currentWeapon && weaponGrenade != null &&
                        (weaponGrenade.magCurrentAmmo > 0
                        && (currentWeapon is W_Crowbar || currentWeapon.magCurrentAmmo <= 0)) && !changeWeaponDelay)
                    {
                        stateMachine.ChangeWeapon(weaponGrenade);
                        break;
                    }

                    weaponCrowbar = otherWeapon.OfType<W_Crowbar>().FirstOrDefault();
                    if (currentWeapon && currentWeapon.magCurrentAmmo <= 0 && !changeWeaponDelay) // до сюда доходит, если вообще не осталось патронов или гранат
                    {
                        stateMachine.ChangeWeapon(weaponCrowbar);
                        break;
                    }
                    break;
                default:
                    otherWeapon = weapons.Where(weapon => weapon != currentWeapon);

                    weaponRifle = otherWeapon.OfType<W_Rifle>().FirstOrDefault();
                    if (currentWeapon && weaponRifle != null &&
                        ((weaponRifle.magCurrentAmmo / weaponRifle.magMaxAmmo > 0f
                        && currentWeapon is W_Pistol && currentWeapon.magCurrentAmmo / currentWeapon.magMaxAmmo <= 0f)
                        || (weaponRifle.magCurrentAmmo / weaponRifle.magMaxAmmo > 0f && (currentWeapon is W_Crowbar || currentWeapon is W_Grenade))) && !changeWeaponDelay)
                    {
                        stateMachine.ChangeWeapon(weaponRifle);
                        break;
                    }

                    weaponPistol = otherWeapon.OfType<W_Pistol>().FirstOrDefault();
                    if (currentWeapon && weaponPistol != null &&
                        ((weaponPistol.magCurrentAmmo / weaponPistol.magMaxAmmo > 0f
                        && currentWeapon is W_Rifle && currentWeapon.magCurrentAmmo / currentWeapon.magMaxAmmo <= 0f)
                        || (weaponPistol.magCurrentAmmo / weaponPistol.magMaxAmmo > 0f && (currentWeapon is W_Crowbar || currentWeapon is W_Grenade))) && !changeWeaponDelay)
                    {
                        stateMachine.ChangeWeapon(weaponPistol);
                        break;
                    }

                    weaponGrenade = otherWeapon.OfType<W_Grenade>().FirstOrDefault();
                    if (currentWeapon && weaponGrenade != null &&
                        weaponGrenade.magCurrentAmmo > 0
                        && (currentWeapon is W_Crowbar || currentWeapon.magCurrentAmmo <= 0)
                        && stateMachine.currentTarget && stateMachine.distanceToTarget > 6f && !changeWeaponDelay)
                    {
                        stateMachine.ChangeWeapon(weaponGrenade);
                        break;
                    }

                    if (currentWeapon && currentWeapon.isReloadable && currentWeapon.currentAmmo > 0 && currentWeapon.magCurrentAmmo / currentWeapon.magMaxAmmo <= 0f
                        && (!stateMachine.currentTarget || (stateMachine.currentTarget && stateMachine.distanceToTarget >= 30f)) && !changeWeaponDelay)
                    {
                        stateMachine.Reload();
                        break;
                    }

                    weaponCrowbar = otherWeapon.OfType<W_Crowbar>().FirstOrDefault();
                    if (currentWeapon && currentWeapon.magCurrentAmmo <= 0 && !changeWeaponDelay) // до сюда доходит, если вообще не осталось патронов или гранат
                    {
                        stateMachine.ChangeWeapon(weaponCrowbar);
                        break;
                    }
                    break;
            }
        }
    }
}
