using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.PlayerSettings;

public struct LastKnownTargetTransform
{
    public Vector3 position;
    public Quaternion rotation;
}

public class FSMEnemy : MonoBehaviour, ITarget
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

    [Header("Movement")]
    public float speed;
    public float rotationSpeed;

    // Components
    [HideInInspector]
    public LevelEnv levelEnv {get; set;}

    [HideInInspector]
    public Rigidbody rb;

    [HideInInspector]
    public FieldOfView fieldOfView;

    // Weapon
    public List<Weapon> weapons { get; set; }
    public Weapon currentWeapon { get; set; }
    public Transform weaponHolder { get; set; }
    WeaponHelper weaponHelper;

    // Navigation
    [HideInInspector]
    public NavMeshAgent navMeshAgent;

    // Target
    public Transform currentTarget { get; set; }
    public LastKnownTargetTransform lastKnownTargetTransform;
    public float distanceToTarget;
    public int enemySearchRadius = 20;

    [Header("Bools")]
    public bool isDead { get; set; } = true;

    public float timeSearch = 0f; // при поиске
    public float timeSearchMax = 10f;
    
    // для отладки
    [Header("Debug")]
    public bool canMove = true;
    public bool canAttack = true;
    public bool canPickupItems = true;
    public bool canPickupWeapons = true;
    public bool isInvisible = false;
    public bool godMode = false;

    [Header("FSM")]
    [SerializeField] private BaseState _initialState;
    public BaseState CurrentState { get; set; }
    public bool isPatrolling = true;
    public bool takenDamage = false;
    public float takenDamageTimer = 0f;
    public float takenDamageTimerMax = 1f;
    public bool isSearching = false;
    public bool isPickingUpItem = false;
    public bool itemPickuped = false;
    public Coroutine attackCoroutine;
    public bool isAttacking = false;
    public bool changeWeaponDelay = false;
    public float changeWeaponTime = 0f;
    public float changeWeaponTimeMax = 2f;

    // Item Related
    [HideInInspector]
    public ItemHelper itemHelper;
    public Coroutine pickingCoroutine { get; set; }
    public Vector3 weaponPointPistol { get; set; } = Vector3.zero;
    public Vector3 weaponPointRifle { get; set; } = Vector3.zero;
    public Vector3 weaponPointGrenade { get; set; } = Vector3.zero;
    public Vector3 healthPackPoint { get; set; } = Vector3.zero;
    public Vector3 armorPackPoint { get; set; } = Vector3.zero;
    public Vector3 ammoPackPointPistol { get; set; } = Vector3.zero;
    public Vector3 ammoPackPointRifle { get; set; } = Vector3.zero;
    public Vector3 ammoPackPointGrenade { get; set; } = Vector3.zero;
    public bool itemPickupDelay = false;
    public float itemPickupDelayTime = 0f;
    public float itemPickupDelayTimeMax = 1f;


    // Hiding
    public Coroutine hidingCoroutine;
    public Collider[] Colliders = new Collider[10];
    public bool isHiding = false;
    public float timeHide = 0f; // при уходе в укрытие
    public float timeHideMax = 10f;

    // оптимизация, сохраняются в словаре полученные компоненты
    private Dictionary<Type, Component> _cachedComponents;
    public new T GetComponent<T>() where T : Component
    {
        if (_cachedComponents.ContainsKey(typeof(T)))
            return _cachedComponents[typeof(T)] as T;

        var component = base.GetComponent<T>();
        if (component != null)
        {
            _cachedComponents.Add(typeof(T), component);
        }
        return component;
    }

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
    // ITarget END
    // =================================================================================================================

    void LastKnownTargetTransform(Vector3 position, Quaternion rotation)
    {
        lastKnownTargetTransform.position = position;
        lastKnownTargetTransform.rotation = rotation;
    }

    public bool IsWalkPointReached()
    {
        if (!navMeshAgent.pathPending)
        {
            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public Vector3 RandomWalkPoint()
    {
        int safeTries = 1000;
        while (safeTries > 0)
        {
            float x = UnityEngine.Random.Range(-49, 49) + UnityEngine.Random.Range(-1, 1) / 2;
            float z = UnityEngine.Random.Range(-49, 49) + UnityEngine.Random.Range(-1, 1) / 2;
            Vector3 newWalkPoint = new Vector3(x, 0, z);

            if (levelEnv.IsWalkable(newWalkPoint)) { return newWalkPoint; }

            safeTries--;
        }

        Debug.Assert(false, "Couldn't find newWalkPoint");
        return Vector3.zero;
    }

    public Vector3 RandomSearchPoint()
    {
        int safeTries = 1000;
        while (safeTries > 0)
        {
            float x = UnityEngine.Random.Range(-enemySearchRadius, enemySearchRadius) + UnityEngine.Random.Range(-1, 1) / 2;
            float z = UnityEngine.Random.Range(-enemySearchRadius, enemySearchRadius) + UnityEngine.Random.Range(-1, 1) / 2;
            Vector3 newWalkPoint = new Vector3(x, 0, z) + lastKnownTargetTransform.position;

            if (levelEnv.IsWalkable(newWalkPoint)) { return newWalkPoint; }

            safeTries--;
        }

        Debug.Assert(false, "Couldn't find newWalkPoint");
        return Vector3.zero;
    }

    void ChangeLayerMask(string layerMask)
    {
        gameObject.layer = LayerMask.NameToLayer(layerMask);
        foreach (Transform child in transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer(layerMask);
        }
    }

    public virtual void Die(ITarget source)
    {
        isDead = true;
        ChangeLayerMask("Ignore Raycast");

        Debug.Log(gameObject.name + " has died!");

#pragma warning disable CS0252 // Возможно, использовано непреднамеренное сравнение ссылок: для левой стороны требуется приведение
        if (source != null && source != this) source.AddScore();
        if (source == this) score--;
#pragma warning restore CS0252 // Возможно, использовано непреднамеренное сравнение ссылок: для левой стороны требуется приведение

        StartCoroutine(Respawn());
    }

    public void TakeDamage(float damage, ITarget source)
    {
        if (!godMode)
        {
            if (currentArmor > 0f)
            {
                float armorDamage = damage - damage / 5;
                float healthDamage = damage / 5;

                currentArmor -= armorDamage; // даже если имеется хотя бы 1 единица брони, то урон будет сниженным
                currentHealth -= healthDamage;

                Mathf.Clamp(currentArmor, 0f, maxArmor);
            }

            else { currentHealth -= damage; }

            if (currentHealth <= 0f)
            {
                Die(source);
            }
            else
            {
#pragma warning disable CS0252
                if (source != null && source != this)
                {
#pragma warning restore CS0252
                    lastKnownTargetTransform.position = source.GetGameObject().transform.position;
                    takenDamage = true;
                }
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

    public void AddWeapon(Weapon weapon) { weaponHelper.AddWeapon(weapon); }

    public void Attack() { weaponHelper.Attack(fieldOfView.transform.position, fieldOfView.transform.forward); }

    public void Reload() { weaponHelper.Reload(); }

    public void ChangeWeapon(int value) 
    {
        weaponHelper.ChangeWeapon(value);
        StopAttackEnemy();
        changeWeaponDelay = true;
    }
    public void ChangeWeapon(Weapon weapon) 
    { 
        weaponHelper.ChangeWeapon(weapon);
        StopAttackEnemy();
        changeWeaponDelay = true;
    } 

    IEnumerator Respawn()
    {
        navMeshAgent.isStopped = true;

        //reset bools
        StopAttackEnemy();
        StopHide();

        isPatrolling = true;
        takenDamage = false;
        takenDamageTimer = 0f;
        isSearching = false;
        isPickingUpItem = false;
        itemPickuped = false;
        timeHide = 0f;
        timeSearch = 0f;

        yield return new WaitForSeconds(3f);

        SpawnHelper.SpawnInRandomPosition(this);
        SpawnHelper.ResetState(this);

        ChangeLayerMask("Target");
        CurrentState = _initialState;
        navMeshAgent.isStopped = false;
    }

    public void StopAttackEnemy()
    {
        isAttacking = false;
        navMeshAgent.updateRotation = true;
        navMeshAgent.angularSpeed = rotationSpeed;
        
        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);
    }

    void UpdateVisibleTarget()
    {
        if (fieldOfView.visibleTargets.Count > 0)
        {
            currentTarget = fieldOfView.visibleTargets[0];
            distanceToTarget = Vector3.Distance(currentTarget.transform.position, transform.position);
        }
        else
        {
            currentTarget = null;
            distanceToTarget = -1f;
        }
    }

    public void StopHide()
    {
        isHiding = false;

        if (hidingCoroutine != null)
            StopCoroutine(hidingCoroutine);
    }

    void Awake()
    {
        team = _team;

        _cachedComponents = new Dictionary<Type, Component>();

        levelEnv = GetComponentInParent<LevelEnv>();
        rb = GetComponent<Rigidbody>();

        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = speed;
        navMeshAgent.angularSpeed = rotationSpeed;

        fieldOfView = GetComponentInChildren<FieldOfView>();
        fieldOfView.self = this;

        weapons = new List<Weapon>() { };
        weaponHelper = GetComponent<WeaponHelper>();
        weaponHelper.SetInterface(this);
        weaponHolder = gameObject.transform.Find("WeaponHolder");

        itemHelper = GetComponent<ItemHelper>();
        itemHelper.SetTarget(this);
        pickingCoroutine = StartCoroutine(itemHelper.ItemPoint());

        CurrentState = _initialState;
        
    }

    void Start()
    {
        StartCoroutine(Respawn());
    }

    void Update()
    {
        UpdateVisibleTarget();
        fieldOfView.transform.rotation = transform.rotation;
        Debug.DrawRay(fieldOfView.transform.position, fieldOfView.transform.forward * fieldOfView.viewRadius, Color.yellow);

        if (isSearching)
        {
            timeSearch += Time.deltaTime;
        }

        if (timeSearch >= timeSearchMax + 2f) 
        {
            timeSearch = 0;
        }

        if (isHiding)
        {
            timeHide += Time.deltaTime;
        }

        if (timeHide >= timeHideMax + 2f)
        {
            timeHide = 0;
        }

        if (takenDamage) { takenDamageTimer += Time.deltaTime; }
        if (takenDamageTimer > takenDamageTimerMax)
        {
            takenDamageTimer = 0;
            takenDamage = false;
        }

        if (changeWeaponDelay) { changeWeaponTime += Time.deltaTime; }
        if (changeWeaponTime >= changeWeaponTimeMax) 
        {
            changeWeaponTime = 0;
            changeWeaponDelay = false;
        }

        if (itemPickupDelay) { itemPickupDelayTime += Time.deltaTime; }
        if (itemPickupDelayTime >= itemPickupDelayTimeMax) 
        {
            itemPickupDelay = false;
            itemPickupDelayTime = 0;
        }

        if (!isDead)
            CurrentState.Execute(this);
    }
}
