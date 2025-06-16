using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour, ITarget
{
    [Header("Scoreboard")]
    public int _team;
    public int team { get; set; }
    public int score { get; set; }
    public int death { get; set; }

    [Header("Health")]
    public float maxHealth { get; } = 100f;
    public float currentHealth { get; set; }
    public float maxArmor = 100f;
    public float currentArmor { get; set; }

    // combat
    public List<Weapon> weapons { get; set; }
    public Weapon currentWeapon { get; set; }

    [Header("States")]
    public bool isDead { get; set; } = false;
    [SerializeField] bool isInvisible = false;
    [SerializeField] bool godMode = false;
    public bool canMove;

    // component
    Transform playerCamera;
    PlayerUI playerUI;
    public Transform weaponHolder { get; set;}
    WeaponHelper weaponHelper;

    // =================================================================================================================
    // ITarget BEGIN
    void ITarget.AddScore() { score++; }

    bool ITarget.IsEnemy(ITarget target) { return target.team != team; }
    
    GameObject ITarget.EquipWeaponPrefab(GameObject prefab) { return weaponHelper.EquipWeaponPrefab(prefab); }
    bool ITarget.AddWeapon(Weapon weapon) { return weaponHelper.AddWeaponBool(weapon); }
    bool ITarget.AddAmmo(Item.AmmoType ammoType) { return weaponHelper.AddAmmo(ammoType); }

    bool ITarget.AddHealth()
    {
        bool result = false;
        if (CheckHealth())
        {
            result = true;
            AddHealth(25f);
        }
        return result;
    }

    bool ITarget.AddArmor()
    {
        bool result = false;
        if (CheckArmor())
        {
            result = true;
            AddArmor(25f);
        }
        return result;
    }

    void ITarget.TakeDamage(float damage, ITarget source) { TakeDamage(damage, source); }

    public GameObject GetGameObject() => gameObject;

    // Trash FIX ME
    LevelEnv ITarget.levelEnv { get; set; }
    Coroutine ITarget.pickingCoroutine { get; set; }
    Vector3 ITarget.weaponPointPistol {  get; set; }
    Vector3 ITarget.weaponPointRifle { get; set; }
    Vector3 ITarget.weaponPointGrenade { get; set; }
    Vector3 ITarget.healthPackPoint { get; set; }
    Vector3 ITarget.armorPackPoint { get; set; }
    Vector3 ITarget.ammoPackPointPistol { get; set; }
    Vector3 ITarget.ammoPackPointRifle { get; set; }
    Vector3 ITarget.ammoPackPointGrenade { get; set; }
    // ITarget END
    // =================================================================================================================

    void ChangeLayerMask(string layerMask)
    {
        gameObject.layer = LayerMask.NameToLayer(layerMask);
        foreach (Transform child in transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer(layerMask);
        }
    }

    public void Die(ITarget source)
    {
        isDead = true;
        
        ChangeLayerMask("Ignore Raycast");
        
        // Анимация смерти, выпадение лута и т.д.
        Debug.Log(gameObject.name + " has died!");

#pragma warning disable CS0252 // Возможно, использовано непреднамеренное сравнение ссылок: для левой стороны требуется приведение
        if (source != null && source != this) source.AddScore();
#pragma warning restore CS0252 // Возможно, использовано непреднамеренное сравнение ссылок: для левой стороны требуется приведение

        StartCoroutine(Respawn());
    }

    public void TakeDamage(float damage, ITarget source)
    {
        if (!godMode)
        {
            if ((int)currentArmor > 0)
            {
                float armorDamage = damage - damage / 5;
                float healthDamage = damage / 5;

                if (armorDamage > currentArmor) { healthDamage += armorDamage - currentArmor; }
                
                currentArmor -= armorDamage; // даже если имеется хотя бы 1 единица брони, то урон будет сниженным
                currentHealth -= healthDamage;

                Mathf.Clamp(currentArmor, 0f, maxArmor);
            }

            else { currentHealth -= damage; }

            if ((int)currentHealth <= 0)
            {
                Die(source);
            }
            else
            {
                // Реакция на получение урона (например, анимация)
            }
        }

    }

    public bool CheckHealth() { return (currentHealth < maxHealth) ? true : false; }

    public bool CheckArmor() { return (currentArmor < maxArmor) ? true : false; }

    public void AddHealth(float health)
    {
        currentHealth += health;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    public void AddArmor(float armor)
    {
        currentArmor += armor;
        currentArmor = Mathf.Clamp(currentArmor, 0f, maxArmor);
    }

    // Weapon related
    public void AddWeapon(Weapon weapon) { weaponHelper.AddWeapon(weapon); }

    public void Attack() { weaponHelper.Attack(playerCamera.position, playerCamera.forward); }

    public void Reload() { weaponHelper.Reload(); }

    public void ChangeWeapon(int value) { weaponHelper.ChangeWeapon(value); }

    IEnumerator Respawn()
    {
        canMove = false;
        yield return new WaitForSeconds(3f);

        SpawnHelper.SpawnInRandomPosition(this);
        SpawnHelper.ResetState(this);
        gameObject.GetComponent<PlayerControl>().OnSpawn();
        ChangeLayerMask("Target");
        canMove = true;
    }

    void UpdatePlayerUI()
    {
        playerUI.SetHealth((int)currentHealth);
        playerUI.SetArmor((int)currentArmor);
        if (currentWeapon != null) { playerUI.SetAmmo(currentWeapon.magCurrentAmmo, currentWeapon.currentAmmo); }
        else { playerUI.SetAmmo(0, 0); playerUI.HideAmmoCrowbar(); }

        if (currentWeapon is W_Crowbar crowbar) { playerUI.HideAmmoCrowbar(); }
        else if (currentWeapon is W_Grenade grenade) { playerUI.HideAmmoGrenade(); }
        else { playerUI.ShowAmmo(); }
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    void Awake()
    {
        team = _team;

        playerCamera = GameObject.Find("PlayerCamera").transform;
        playerUI = gameObject.GetComponent<PlayerUI>();
        weaponHolder = gameObject.transform.Find("WeaponHolder");
        weapons = new List<Weapon>() { };

        weaponHelper = gameObject.GetComponent<WeaponHelper>();
        weaponHelper.SetInterface(this);
    }

    void Start()
    {
        StartCoroutine(Respawn());
    }

    void Update()
    {
        UpdatePlayerUI();
    }
}
