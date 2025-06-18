using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.AI;

public class MLAgent : Agent, IAgent
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
    float smoothYawChange = 0f;

    // Components
    [HideInInspector]
    public LevelEnv levelEnv { get; set; }

    [HideInInspector]
    public Rigidbody rb;

    [HideInInspector]
    public FieldOfView fieldOfView;

    // Target
    public Transform currentTarget;
    public float distanceToTarget { get; set; } = -1f;
    public LastKnownTargetTransform lastKnownTargetTransform;

    Transform visibleGrenade;
    float distanceToGrenade = -1f;

    // Weapon
    public List<Weapon> weapons { get; set; }
    public Weapon currentWeapon { get; set; }
    public Transform weaponHolder { get; set; }
    WeaponHelper weaponHelper;

    // Navigation
    [HideInInspector]
    public NavMeshAgent navMeshAgent;
    public NavMeshPath path;

    Vector3 walkPoint = Vector3.zero;

    [Header("States")]
    [SerializeField] protected bool isTrainingMode;
    public bool isDead { get; set; } = true;

    public float maxSeeDistance = 150f;

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

    // Hiding
    public Coroutine hidingCoroutine;
    public Collider[] Colliders = new Collider[10];

    [Header("ML-Agents")]
    BehaviorParameters behaviorParameters;

    // =================================================================================================================
    // ITarget BEGIN
    void ITarget.AddScore() { score++; AddReward(6f); }

    bool ITarget.IsEnemy(ITarget target) { return target.team != team; }

    GameObject ITarget.EquipWeaponPrefab(GameObject prefab) { return weaponHelper.EquipWeaponPrefab(prefab); }
    bool ITarget.AddWeapon(Weapon weapon) { return weaponHelper.AddWeaponBool(weapon); }
    bool ITarget.AddAmmo(Item.AmmoType ammoType) 
    {
        var result = weaponHelper.AddAmmo(ammoType);
        if (result) AddReward(0.01f);
        return result; 
    }

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

    void IAgent._AddReward(float reward)
    {
        AddReward(reward);
    }

    void ChangeLayerMask(string layerMask)
    {
        gameObject.layer = LayerMask.NameToLayer(layerMask);
        foreach (Transform child in transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer(layerMask);
        }
    }

    public bool CheckHealth() { return (currentHealth < maxHealth) ? true : false; }

    public bool CheckArmor() { return (currentArmor < maxArmor) ? true : false; }

    public void AddHealth(float health)
    {
        currentHealth += health;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        AddReward(0.2f);
    }

    public void AddArmor(float armor)
    {
        currentArmor += armor;
        currentArmor = Mathf.Clamp(currentArmor, 0f, maxArmor);
        AddReward(0.05f);
    }

    public void TakeDamage(float damage, ITarget source)
    {
        AddReward(-0.005f);
        if (currentArmor > 0f)
        {
            float armorDamage = damage - damage / 5;
            float healthDamage = damage / 5;

            currentArmor -= armorDamage; // даже если имеется хотя бы 1 единица брони, то урон будет сниженным
            currentHealth -= healthDamage;

            currentArmor = Mathf.Clamp(currentArmor, 0f, maxArmor);
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
                lastKnownTargetTransform.position = source.GetGameObject().transform.position;
            }
#pragma warning restore CS0252
        }
    }


    protected virtual void Die(ITarget source)
    {
        AddReward(-1f);

        isDead = true;
        ChangeLayerMask("Ignore Raycast");

        //Debug.Log(gameObject.name + " has died!");

#pragma warning disable CS0252
        if (source != null && source != this) source.AddScore();
        if (source == this) score--;
#pragma warning restore CS0252

        if (isTrainingMode) EndEpisode();

        StartCoroutine(Respawn());
    }

    public void AddWeapon(Weapon weapon) { weaponHelper.AddWeapon(weapon); }

    public void V_Attack(int value) { if (value == 1) Attack(); }

    public void Attack() { weaponHelper.Attack(fieldOfView.transform.position, fieldOfView.transform.forward); }

    public void V_Reload(int value) { if (value == 1) Reload(); }

    public void Reload() { weaponHelper.Reload(); }

    public void ChangeWeapon(int value) { weaponHelper.ChangeWeapon(value); }

    public void ChangeWeapon(Weapon weapon) { weaponHelper.ChangeWeapon(weapon); }

    public void MoveToPos(int x, int z)
    {
        float newX = (float)(-50 + x * 0.5);
        float newZ = (float)(-50 + z * 0.5);

        Vector3 newWalkPoint = new Vector3(newX, 0, newZ) + levelEnv.transform.position;

        if (Vector3.Distance(newWalkPoint, walkPoint) > 10f && !currentTarget) { AddReward(-0.1f); }

        walkPoint = newWalkPoint;

        if (levelEnv.IsWalkable(walkPoint))
        {
            navMeshAgent.SetDestination(walkPoint);

            Collider[] items = Physics.OverlapSphere(walkPoint, 0.5f);
            bool item = false;

            foreach (var _item in items)
            {
                if (_item.GetComponent<Item>() && _item.gameObject.activeSelf) { item = true; break; }
            }

            if (item)
            {
                AddReward(0.1f);
            }
        }
        else
        {
            AddReward(-0.01f);
        }

    }

    public void RotateAgent(int y)
    {
        Vector3 rotationVector = transform.rotation.eulerAngles;

        float yawChange = y - 1;
        smoothYawChange = Mathf.MoveTowards(smoothYawChange, yawChange, 2f * Time.fixedDeltaTime);
        float yaw = rotationVector.y + smoothYawChange * Time.fixedDeltaTime * rotationSpeed;

        transform.rotation = Quaternion.Euler(0, yaw, 0);
    }

    // ======================================================================
    // ML-Agents functions BEGIN
    public override void Initialize()
    {
        team = _team;

        levelEnv = GetComponentInParent<LevelEnv>();
        rb = GetComponent<Rigidbody>();

        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = speed;
        navMeshAgent.updateRotation = false;
        navMeshAgent.angularSpeed = 0f;

        path = new NavMeshPath();

        fieldOfView = GetComponentInChildren<FieldOfView>();
        fieldOfView.self = this;

        weapons = new List<Weapon>() { };
        weaponHelper = GetComponent<WeaponHelper>();
        weaponHelper.SetInterface(this);
        weaponHolder = gameObject.transform.Find("WeaponHolder");

        itemHelper = GetComponent<ItemHelper>();
        itemHelper.SetTarget(this);
        pickingCoroutine = StartCoroutine(itemHelper.ItemPoint());

        behaviorParameters = GetComponent<BehaviorParameters>();
        behaviorParameters.TeamId = team;

        if (!isTrainingMode) MaxStep = 0;
    }

    // Поступили аргументы от нейронной сети для действий
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (isDead) return;

        // выбор маршрута
        // размер от 0 до 200 по X и Z
        // массив точек от -50 до 50, где размер деления 0.5
        MoveToPos(actions.DiscreteActions[0], actions.DiscreteActions[1]);

        // поворот агента
        RotateAgent(actions.DiscreteActions[2]);

        // атака агента
        // 0 - не атаковать, 1 - атаковать
        V_Attack(actions.DiscreteActions[3]);

        // перезарядка оружия
        // 0 - не перезаряжать, 1 - перезарядить
        V_Reload(actions.DiscreteActions[4]);

        // смена оружия
        // 0 - не менять, 1 - монтировка, 2 - пистолет, 3 - автомат, 4 - граната
        ChangeWeapon(actions.DiscreteActions[5]);
    }

    // Прокидываем нужные параметры агента и переменные в нейронную сеть
    public override void CollectObservations(VectorSensor sensor)
    {
        // Navigation
        sensor.AddObservation(transform.position); // 3
        sensor.AddObservation(transform.rotation.y); // 1
        sensor.AddObservation(walkPoint); // 3

        // Health
        sensor.AddObservation(currentHealth); // 1
        sensor.AddObservation(currentArmor); // 1

        // Weapon
        if (currentWeapon is W_Crowbar) sensor.AddObservation(1);
        else if (currentWeapon is W_Pistol) sensor.AddObservation(2);
        else if (currentWeapon is W_Rifle) sensor.AddObservation(3);
        else if (currentWeapon is W_Grenade) sensor.AddObservation(4);
        else sensor.AddObservation(0);
        // 1

        if (weapons.Any(w => w.WeaponType == Item.WeaponType.Pistol)) 
        {
            var pistol = weapons.Where(w => w.WeaponType == Item.WeaponType.Pistol).FirstOrDefault();
            var pistolMagAmmo = pistol.magCurrentAmmo;
            var pistolMagMaxAmmo = pistol.magMaxAmmo;
            var pistolCurrenAmmo = pistol.currentAmmo;
            var pistolMaxAmmo = pistol.maxAmmo;
            sensor.AddObservation(2);
            sensor.AddObservation(pistolMagAmmo / pistolMagMaxAmmo);
            sensor.AddObservation(pistolCurrenAmmo / pistolMaxAmmo);
        }
        else
        {
            sensor.AddObservation(0);
            sensor.AddObservation(0);
            sensor.AddObservation(0);
        }
        // 3

        if (weapons.Any(w => w.WeaponType == Item.WeaponType.Rifle))
        {
            var rifle = weapons.Where(w => w.WeaponType == Item.WeaponType.Rifle).FirstOrDefault();
            var rifleMagAmmo = rifle.magCurrentAmmo;
            var rifleMagMaxAmmo = rifle.magMaxAmmo;
            var rifleCurrenAmmo = rifle.currentAmmo;
            var rifleMaxAmmo = rifle.maxAmmo;
            sensor.AddObservation(3);
            sensor.AddObservation(rifleMagAmmo / rifleMagMaxAmmo);
            sensor.AddObservation(rifleCurrenAmmo / rifleMaxAmmo);
        }
        else
        {
            sensor.AddObservation(0);
            sensor.AddObservation(0);
            sensor.AddObservation(0);
        }
        // 3

        if (weapons.Any(w => w.WeaponType == Item.WeaponType.Grenade))
        {
            var grenade = weapons.Where(w => w.WeaponType == Item.WeaponType.Grenade).FirstOrDefault();
            var grenadeMagAmmo = grenade.magCurrentAmmo;
            var grenadeMaxMagAmmo = grenade.magMaxAmmo;
            sensor.AddObservation(4);
            sensor.AddObservation(grenadeMagAmmo / grenadeMaxMagAmmo);
        }
        else
        {
            sensor.AddObservation(0);
            sensor.AddObservation(0);
        }
        // 2

        // Target
        if (currentTarget) sensor.AddObservation(currentTarget.position);
        else sensor.AddObservation(Vector3.zero);
        // 3

        sensor.AddObservation(distanceToTarget); // 1

        if (lastKnownTargetTransform.position != null)
            sensor.AddObservation(lastKnownTargetTransform.position); // 3
        else sensor.AddObservation(Vector3.zero);

        // Grenade
        if (visibleGrenade) sensor.AddObservation(visibleGrenade.position);
        else sensor.AddObservation(Vector3.zero);
        // 3

        sensor.AddObservation(distanceToGrenade); //1

        // Item
        sensor.AddObservation(weaponPointPistol); // 3
        if (weaponPointPistol != Vector3.zero)
        {
            var pathLength = itemHelper.GetPathLength(path, transform.position, weaponPointPistol, navMeshAgent.areaMask);
            sensor.AddObservation(pathLength);
        }
        else sensor.AddObservation(0); // 1

        sensor.AddObservation(weaponPointRifle); // 3
        if (weaponPointRifle != Vector3.zero)
        {
            var pathLength = itemHelper.GetPathLength(path, transform.position, weaponPointRifle, navMeshAgent.areaMask);
            sensor.AddObservation(pathLength);
        }
        else sensor.AddObservation(0); // 1

        sensor.AddObservation(weaponPointGrenade); // 3
        if (weaponPointGrenade != Vector3.zero)
        {
            var pathLength = itemHelper.GetPathLength(path, transform.position, weaponPointGrenade, navMeshAgent.areaMask);
            sensor.AddObservation(pathLength);
        }
        else sensor.AddObservation(0); // 1

        sensor.AddObservation(healthPackPoint); // 3
        if (healthPackPoint != Vector3.zero)
        {
            var pathLength = itemHelper.GetPathLength(path, transform.position, healthPackPoint, navMeshAgent.areaMask);
            sensor.AddObservation(pathLength);
        }
        else sensor.AddObservation(0); // 1

        sensor.AddObservation(armorPackPoint); // 3
        if (armorPackPoint != Vector3.zero)
        {
            var pathLength = itemHelper.GetPathLength(path, transform.position, armorPackPoint, navMeshAgent.areaMask);
            sensor.AddObservation(pathLength);
        }
        else sensor.AddObservation(0); // 1

        sensor.AddObservation(ammoPackPointPistol); // 3
        if (ammoPackPointPistol != Vector3.zero)
        {
            var pathLength = itemHelper.GetPathLength(path, transform.position, ammoPackPointPistol, navMeshAgent.areaMask);
            sensor.AddObservation(pathLength);
        }
        else sensor.AddObservation(0); // 1

        sensor.AddObservation(ammoPackPointRifle); // 3
        if (ammoPackPointRifle != Vector3.zero)
        {
            var pathLength = itemHelper.GetPathLength(path, transform.position, ammoPackPointRifle, navMeshAgent.areaMask);
            sensor.AddObservation(pathLength);
        }
        else sensor.AddObservation(0); // 1

        sensor.AddObservation(ammoPackPointGrenade); // 3
        if (ammoPackPointGrenade != Vector3.zero)
        {
            var pathLength = itemHelper.GetPathLength(path, transform.position, ammoPackPointGrenade, navMeshAgent.areaMask);
            sensor.AddObservation(pathLength);
        }
        else sensor.AddObservation(0); // 1

    }

    // Старт эпизода
    public override void OnEpisodeBegin()
    {
        StartCoroutine(Respawn());
    }

    // ML-Agents functions END
    // ======================================================================

    IEnumerator Respawn()
    {
        navMeshAgent.isStopped = true;

        yield return new WaitForSeconds(3f);

        SpawnHelper.SpawnInRandomPosition(this);
        walkPoint = transform.position;
        SpawnHelper.ResetState(this);

        ChangeLayerMask("Target");
        navMeshAgent.isStopped = false;
    }

    void UpdateVisibleTarget()
    {
        if (fieldOfView.visibleTargets.Count > 0)
        {
            currentTarget = fieldOfView.visibleTargets[0];
            distanceToTarget = Vector3.Distance(currentTarget.position, transform.position);

            if (distanceToTarget > 0)
            {
                AddReward(1f / distanceToTarget);
            }
        }
        else
        {
            currentTarget = null;
            distanceToTarget = -1f;
        }
    }

    void UpdateVisibleGrenade()
    {
        if (fieldOfView.visibleGrenades.Count > 0)
        {
            visibleGrenade = fieldOfView.visibleGrenades[0];
            distanceToGrenade = Vector3.Distance(visibleGrenade.position, transform.position);
        }
        else
        {
            visibleGrenade = null;
            distanceToGrenade = -1f;
        }
    }

    private void Start()
    {
        Debug.Log("Agent initialized");
        if (Academy.Instance.IsCommunicatorOn)
            Debug.Log("Communicator is ON");
    }

    private void Update()
    {
        UpdateVisibleTarget();
        UpdateVisibleGrenade();
        fieldOfView.transform.rotation = transform.rotation;
        Debug.DrawRay(fieldOfView.transform.position, fieldOfView.transform.forward * fieldOfView.viewRadius, Color.blue);
    }
}