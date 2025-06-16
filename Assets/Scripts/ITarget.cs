using System.Collections.Generic;
using UnityEngine;

public interface ITarget
{
    GameObject GetGameObject();

    // scoreboard
    int team { get; set; }
    int score { get; set; }
    int death { get; }

    // params
    float currentHealth { get; set; }
    float maxHealth { get; }
    float currentArmor { get; set; }
    List<Weapon> weapons { get; set; }
    Weapon currentWeapon { get; set; }
    Transform weaponHolder { get; }

    // flags
    bool isDead { get; set; }

    // methods
    bool IsEnemy(ITarget target);
    void AddScore();
    void TakeDamage(float damage, ITarget source);
    bool AddWeapon(Weapon weapon);
    void ChangeWeapon(int value);
    bool AddAmmo(Item.AmmoType ammoType);
    bool AddHealth();
    bool AddArmor();
    GameObject EquipWeaponPrefab(GameObject prefab);

    //ItemHelper
    public Coroutine pickingCoroutine { get; set; }
    public Vector3 weaponPointPistol { get; set; }
    public Vector3 weaponPointRifle { get; set; }
    public Vector3 weaponPointGrenade { get; set; }
    public Vector3 healthPackPoint { get; set; }
    public Vector3 armorPackPoint { get; set; }
    public Vector3 ammoPackPointPistol { get; set; }
    public Vector3 ammoPackPointRifle { get; set; }
    public Vector3 ammoPackPointGrenade { get; set; }

    public LevelEnv levelEnv { get; set; }
}
