/*

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.AI;

public class BasicAgent : Agent, IAgent
{
    [Header("Scoreboard")]
    public byte team { get; set; }
    public int score { get; set; }
    public int death { get; set; }

    [Header("Health")]
    public float maxHealth { get; } = 100f;
    public float currentHealth { get; set; }
    public float maxArmor = 100f;
    public float currentArmor { get; set; }

    [Header("Movement")]
    [SerializeField] protected float speed;
    [SerializeField] protected float rotationSpeed;
    protected float smoothYawChange = 0f;
    protected float smoothPitchChange = 0f;
    [SerializeField] protected float maxPitchAngle = 80f;
    
    // Components
    new Rigidbody rigidbody;
    Camera viewCamera;
    Transform pointOfView;
    protected Transform target;
    FieldOfView fieldOfView;
    Vector3 fovPos;
    Vector3 fovForward;
    public Transform weaponHolder { get; set; }
    WeaponHelper weaponHelper;

    BehaviorParameters behaviorParameters;

    // Navigation
    Vector3 walkPoint;
    bool walkPointSet;
    protected Vector3 lastKnownTargetPosition;
    protected NavMeshAgent navAgent;
    protected NavMeshPath path;
    public LayerMask whatIsGround, whatIsTarget;

    [Header("Combat")]
    public List<Weapon> weapons { get; set; }
    public Weapon currentWeapon { get; set; }

    [Header("States")]
    [SerializeField] protected bool isTrainingMode;
    public bool isDead { get; set; } = false;
    protected bool isAttacking = false;
    protected bool isTargetVisible = false;
    protected float maxSeeDistance;



    // Reward related states
    int R_EmptyGunFire = 1;

    // =================================================================================================================
    // ITarget BEGIN
    byte ITarget.team { get => team; set => team = value; }
    int ITarget.score { get => score; set => score = value; }
    int ITarget.death => death;

    float ITarget.currentHealth { get => currentHealth; set => currentHealth = value; }
    float ITarget.maxHealth => maxHealth;
    List<Weapon> ITarget.weapons { get => weapons; set => weapons = value; }
    Weapon ITarget.currentWeapon { get => currentWeapon; set => currentWeapon = value; }

    bool ITarget.isDead { get => isDead; set => isDead = value; }

    void ITarget.AddScore() { score++; }
    bool ITarget.IsEnemy(ITarget target) { return (target.team == team) ? false : true; }
    void ITarget.TakeDamage(float damage, ITarget source) { TakeDamage(damage, source); }
    bool ITarget.AddWeapon(Weapon weapon)
    {
        bool result = false;
        bool alreadyExists = weapons.Any(w => w.GetType() == weapon.GetType());

        if (alreadyExists) // ���� ������ ��� ����, �� ����� ��������� � ���� ��������
        {
            Weapon neededWeapon;
            if (weapon is W_Pistol pistol)
            {
                neededWeapon = weapons.OfType<W_Pistol>().FirstOrDefault();
                if (neededWeapon != null && neededWeapon.CheckAmmo()) { result = true; neededWeapon.AddAmmo(); }
            }
            else if (weapon is W_Rifle rifle)
            {
                neededWeapon = weapons.OfType<W_Rifle>().FirstOrDefault();
                if (neededWeapon != null && neededWeapon.CheckAmmo()) { result = true; neededWeapon.AddAmmo(); }
            }
            else if (weapon is W_Grenade grenade)
            {
                neededWeapon = weapons.OfType<W_Grenade>().FirstOrDefault();
                if (neededWeapon != null && neededWeapon.CheckAmmo()) { result = true; neededWeapon.AddAmmo(); }
            }
        }
        else if (!alreadyExists) { AddWeapon(weapon); result = true; }

        return result;
    }
    bool ITarget.AddAmmo(Item.AmmoType ammoType)
    {
        bool result = false;
        Weapon neededWeapon;
        switch (ammoType)
        {
            case Item.AmmoType.Pistol:
                neededWeapon = weapons.OfType<W_Pistol>().FirstOrDefault();
                if (neededWeapon != null && neededWeapon.CheckAmmo()) { result = true; neededWeapon.AddAmmo(); }
                break;
            case Item.AmmoType.Rifle:
                neededWeapon = weapons.OfType<W_Rifle>().FirstOrDefault();
                if (neededWeapon != null && neededWeapon.CheckAmmo()) { result = true; neededWeapon.AddAmmo(); }
                break;
        }

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

    GameObject ITarget.EquipWeaponPrefab(GameObject prefab)
    {
        foreach (Transform child in weaponHolder.transform)
        {
            Destroy(child.gameObject);
        }

        Quaternion originalRotation = prefab.transform.rotation;
        Quaternion yRotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        Quaternion yRotation2 = Quaternion.Euler(0, -2.08f, 0);
        Quaternion finalRotation = yRotation * originalRotation * yRotation2;

        GameObject weaponObj = Instantiate(prefab, weaponHolder.position, finalRotation);
        weaponObj.transform.parent = weaponHolder.transform;

        return weaponObj;
    }

    public GameObject GetGameObject() => gameObject;
    // ITarget END
    // =================================================================================================================

    public bool CheckHealth() { return (currentHealth < maxHealth) ? true : false; }

    public bool CheckArmor() { return (currentArmor < maxArmor) ? true : false; }

    public void AddHealth(float health)
    {
        currentHealth += health;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        AddReward(0.05f);
    }

    public void AddArmor(float armor)
    {
        currentArmor += armor;
        currentArmor = Mathf.Clamp(currentArmor, 0f, maxArmor);
        AddReward(0.01f);
    }

    public void TakeDamage(float damage, ITarget source)
    {
        AddReward(-0.02f);
        if (currentArmor > 0f)
        {
            float armorDamage = damage - damage / 5;
            float healthDamage = damage / 5;

            currentArmor -= armorDamage; // ���� ���� ������� ���� �� 1 ������� �����, �� ���� ����� ���������
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
            // ������� �� ��������� ����� (��������, ��������)
        }
    }


    protected virtual void Die(ITarget source)
    {
        AddReward(-2f);

        isDead = true;
        navAgent.isStopped = true;
        
        rigidbody.Sleep();

        // �������� ������, ��������� ���� � �.�.

        //Destroy(gameObject, 2f);
        Debug.Log(gameObject.name + " has died!");

        #pragma warning disable CS0252 // ��������, ������������ ���������������� ��������� ������: ��� ����� ������� ��������� ����������
        if (source != null && source != this) source.AddScore();
        #pragma warning restore CS0252 // ��������, ������������ ���������������� ��������� ������: ��� ����� ������� ��������� ����������
    }

    protected void HitEnemy(ITarget target)
    {
        target.TakeDamage(currentWeapon.damage, this);
        AddReward(0.15f);
    }

    // ������� ������ � ��������� ����������� ����� �� ������
    private void SpawnInRandomPosition()
    {
        bool safePositionFound = false;
        int attemptsRemainig = 100; // �������� ������� �����

        Vector3 potentialPosition = Vector3.zero;
        Quaternion potentialRotation = new Quaternion();

        // ����, � ������� ���� ������� ��� ������
        while (!safePositionFound && attemptsRemainig > 0)
        {
            attemptsRemainig--;

            //float radius = UnityEngine.Random.Range(2f, 20f);
            //Quaternion direction = Quaternion.Euler(0f, UnityEngine.Random.Range(-180f, 180f), 0f);
            float x = UnityEngine.Random.Range(-49, 49) + UnityEngine.Random.Range(-1, 1) / 2;
            float z = UnityEngine.Random.Range(-49, 49) + UnityEngine.Random.Range(-1, 1) / 2;

            potentialPosition = new Vector3(x, 1, z);

            float yaw = UnityEngine.Random.Range(-180f, 180f);
            potentialRotation = Quaternion.Euler(0f, yaw, 0f);

            Collider[] colliders = Physics.OverlapSphere(potentialPosition, 0.5f);
            safePositionFound = colliders.Length == 0;
        }

        if (attemptsRemainig == 0) Debug.Assert(safePositionFound, "���������� ����� ����� ��� ������!");
        else
        {
            transform.position = potentialPosition;
            transform.rotation = potentialRotation;
        }
    }

    // ����� ���� (target) �� ���������� �����������
    private void UpdateNearestTarget()
    {
        // ����� �������� ����� ����� ����� ������ 1
        //foreach (Transform target in )
    }

    // ���� ����� �������� ��������
    private void OnTriggerEnter(Collider other)
    {
        // �������, ������, �������
    }

    // ���� ����� ����� ������ ������� ����
    private void OnTriggerStay(Collider other)
    {

    }

    // ���� ����� �������� ����-�� (�� �������)
    private void OnCollisionEnter(Collision collision)
    {

    }

    public void AddWeapon(Weapon weapon)
    {
        bool alreadyExists = weapons.Any(w => w.GetType() == weapon.GetType());

        if (!alreadyExists) 
        { 
            weapons.Add(weapon);
            AddReward(0.1f);
        }
    }

    // ============================================================================================================
    // �������� ����� OnActionReceived BEGIN
    public void MoveToPos(int x, int z)
    {
        //navAgent.isStopped = false;
        float newX = (float)(-50 + x * 0.5);
        float newZ = (float)(-50 + z * 0.5);

        walkPoint = new Vector3(newX, 0, newZ);
        /* TODO
         ��������, ����� ����� �������� ������ 1 �����

        // ������ ������ ������ ��������� �����
        //float y = Terrain.activeTerrain.SampleHeight(walkPoint) + Terrain.activeTerrain.transform.position.y;

        //walkPoint = new Vector3(walkPoint.x, y, walkPoint.z);
        */

        // ���� �������� ����� �� �����
        // TODO ����� �������������� ����� ���� ���� ��������

/*
        if (navAgent.CalculatePath(walkPoint, path) && path.status == NavMeshPathStatus.PathComplete)
        {
            navAgent.destination = walkPoint;
        }
        else
        {
            AddReward(-0.1f);
        }

        // �������� �����, ���� ����� �������� ���� � ��������, ������� ��� �� �����

    }

    public void RotateAgent(int x, int y)
    {
        Vector3 rotationVector = transform.rotation.eulerAngles;

        float pitchChange = x - 1;
        smoothPitchChange = Mathf.MoveTowards(smoothPitchChange, pitchChange, 2f * Time.fixedDeltaTime);
        float pitch = rotationVector.x + smoothPitchChange * Time.fixedDeltaTime * rotationSpeed;
        if (pitch > 180f) pitch -= 360f;
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        float yawChange = y - 1;
        smoothYawChange = Mathf.MoveTowards(smoothYawChange, yawChange, 2f * Time.fixedDeltaTime);
        float yaw = rotationVector.y + smoothYawChange * Time.fixedDeltaTime * rotationSpeed;

        transform.rotation = Quaternion.Euler(0, yaw, 0);
        //pointOfView.localRotation = Quaternion.Euler(pitch, 0, 0);
        fieldOfView.ChangeLocalX(pitch);
    }

    public void Attack(int value)
    {
        if (value == 1)
        {
            if (R_EmptyGunFire == 0) AddReward(-0.1f);
            if (currentWeapon.currentAmmo == 0) R_EmptyGunFire--;
            
            currentWeapon.Attack(this, fovPos, fovForward);
            
        }
        // �������� �����, ���� ����� ������ ��� ������� + ���� ��������� ��������� ������ �������
    }
   
    public void Reload(int value)
    {
        if (value == 1)
        {
            if (currentWeapon.GetType() != typeof(W_Crowbar))
            {
                if (isTargetVisible && currentWeapon.currentAmmo / currentWeapon.maxAmmo > 0.33f) AddReward(-0.2f);
            }

            currentWeapon.CReload();
        }
        // �������� �����, ���� ����� ������ ��� �������������� + ���� ����� �������������� ���� � �������� ��� �������� ���� ���������
    }

    public void ChangeWeapon(int value)
    {
        switch (value)
        {
            // ����������
            case 1:
                Weapon neededWeapon = weapons.OfType<W_Crowbar>().FirstOrDefault();
                if (neededWeapon && currentWeapon != neededWeapon)
                {
                    currentWeapon = neededWeapon;
                    // TODO �������� �������� ����� �����������
                }
                break;
        }

        // �������� �����, ���� ����� ������ ��� ������ ������
    }

    // �������� ����� OnActionReceived END
    //===============================================================================================================================

    // ======================================================================
    // ML-Agents functions BEGIN
    // ������ void Start()
    public override void Initialize()
    {
        rigidbody = GetComponent<Rigidbody>();
        pointOfView = this.transform.Find("PointOfView");
        behaviorParameters = GetComponent<BehaviorParameters>();
        behaviorParameters.TeamId = team;

        // ��� ����������� �� ������������ ������� ��� �������������� ������
        if (!isTrainingMode) MaxStep = 0;


    }

    // ��������� ��������� �� ��������� ���� ��� ��������
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (isDead) return;

        // ����� ��������
        // ������ �� 0 �� 200 �� X � Z
        // ������ ����� �� -50 �� 50, ��� ������ ������� 0.5
        MoveToPos(actions.DiscreteActions[0], actions.DiscreteActions[1]);

        // ������� ������
        RotateAgent(actions.DiscreteActions[2], actions.DiscreteActions[3]);

        // ����� ������
        // 0 - �� ���������, 1 - ���������
        Attack(actions.DiscreteActions[4]);

        // ����������� ������
        // 0 - �� ������������, 1 - ������������
        Reload(actions.DiscreteActions[5]);

        // ����� ������
        // 0 - �� ������, 1 - ����������, 2 - ��������, 3 - �������, 4 - �������
        ChangeWeapon(actions.DiscreteActions[6]);
    }

    // ����������� ������ ��������� ������ � ���������� � ��������� ����
    public override void CollectObservations(VectorSensor sensor)
    {

        //sensor.AddObservation(transform.localRotation.normalized);
        sensor.AddObservation(transform.localRotation.normalized.x);
        sensor.AddObservation(transform.localRotation.normalized.y);
        sensor.AddObservation(transform.localRotation.normalized.z);

    /*

        /*sensor.AddObservation(currentHealth);
        sensor.AddObservation(speed);
        sensor.AddObservation(currentWeapon);
        sensor.AddObservation(maxSeeDistance);
        sensor.AddObservation(target);
        sensor.AddObservation(lastKnownTargetPosition);
        sensor.AddObservation(isAttacking);
        sensor.AddObservation(isDead);
        sensor.AddObservation(isTargetVisible);
        sensor.AddObservation(team);
        */

    //}

/*

    // ���� Behavior Type ��������� �� "Heuristic Only", �� ����� ����������
    // ������ ������� � �������� ����� ���������� � ������� OnActionReceived ������ ��������� ����
    // !!! ��� rule-based �� ��� ��� ������ ����� ����� �������
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;

        if (Input.GetKey(KeyCode.Q)) // ��������� ����� ����� ����������
        {
            discreteActions[0] = UnityEngine.Random.Range(0, 199);
            discreteActions[1] = UnityEngine.Random.Range(0, 199);
            discreteActions[2] = 1;
            discreteActions[3] = 1;
            discreteActions[4] = 0;
            discreteActions[5] = 0;
            discreteActions[6] = 0;
            Debug.Log("new walkPoint" + discreteActions[0] + " " + discreteActions[1]);
        }
        else
        {
            discreteActions[0] = (int)walkPoint.x;
            discreteActions[1] = (int)walkPoint.z;
            discreteActions[2] = 1;
            discreteActions[3] = 1;
            discreteActions[4] = 0;
            discreteActions[5] = 0;
            discreteActions[6] = 0;
        }
        if (Input.GetKey(KeyCode.W)) 
        {
            discreteActions[0] = (int)walkPoint.x;
            discreteActions[1] = (int)walkPoint.z;
            discreteActions[2] = 1;
            discreteActions[3] = 1;
            discreteActions[4] = 1; // ���������
            discreteActions[5] = 0;
            discreteActions[6] = 0;
        }
        if (Input.GetKey(KeyCode.R))
        {
            discreteActions[0] = (int)walkPoint.x;
            discreteActions[1] = (int)walkPoint.z;
            discreteActions[2] = 1;
            discreteActions[3] = 1;
            discreteActions[4] = 0;
            discreteActions[5] = 1; // �����������
            discreteActions[6] = 0;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            discreteActions[0] = (int)walkPoint.x;
            discreteActions[1] = (int)walkPoint.z;
            discreteActions[2] = 2;
            discreteActions[3] = 1;
            discreteActions[4] = 0;
            discreteActions[5] = 0;
            discreteActions[6] = 0;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            discreteActions[0] = (int)walkPoint.x;
            discreteActions[1] = (int)walkPoint.z;
            discreteActions[2] = 0;
            discreteActions[3] = 1;
            discreteActions[4] = 0;
            discreteActions[5] = 0;
            discreteActions[6] = 0;
        }
        if ((Input.GetKey(KeyCode.LeftArrow)))
        {
            discreteActions[0] = (int)walkPoint.x;
            discreteActions[1] = (int)walkPoint.z;
            discreteActions[2] = 1;
            discreteActions[3] = 0;
            discreteActions[4] = 0;
            discreteActions[5] = 0;
            discreteActions[6] = 0;
        }
        if ((Input.GetKey(KeyCode.RightArrow)))
        {
            discreteActions[0] = (int)walkPoint.x;
            discreteActions[1] = (int)walkPoint.z;
            discreteActions[2] = 1;
            discreteActions[3] = 2;
            discreteActions[4] = 0;
            discreteActions[5] = 0;
            discreteActions[6] = 0;
        }
    }

    // ����� �������
    public override void OnEpisodeBegin()
    {
        behaviorParameters = GetComponent<BehaviorParameters>();
        behaviorParameters.TeamId = team;

        if (isTrainingMode)
        {
            // ������ �������� ������, �������� � ��
        }

        score = 0;
        death = 0;

        rigidbody = GetComponent<Rigidbody>();
        // �������� ��������
        rigidbody.linearVelocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        // �������� ������� ������ �� ��� X
        pointOfView.transform.rotation = Quaternion.Euler(0, rigidbody.rotation.y, 0);
        walkPoint = transform.position;

        // ��������� ����� ���������
        //SpawnInRandomPosition();
    }

    // ML-Agents functions END
    // ======================================================================

    protected virtual void Start()
    {
        pointOfView = this.transform.Find("PointOfView");
        rigidbody = GetComponent<Rigidbody>();
        fieldOfView = pointOfView.GetComponent<FieldOfView>();
        currentHealth = maxHealth;

        // ��������� NavMeshAgent
        navAgent = GetComponent<NavMeshAgent>();
        //navAgent.angularSpeed = 0;
        navAgent.speed = speed;
        navAgent.angularSpeed = rotationSpeed;

        path = new NavMeshPath();
        //playerTarget = GameObject.FindGameObjectWithTag("Player").transform;

        viewCamera = Camera.main;

        weapons = new List<Weapon>() { gameObject.AddComponent<W_Crowbar>() };

        currentWeapon = weapons[0];
    }

    protected void Update()
    {
        if (isDead) return;
        fovPos = fieldOfView.transform.position;
        fovForward = Quaternion.Inverse(fieldOfView.transform.localRotation) * Vector3.forward;

        Debug.DrawRay(fovPos, fovForward * fieldOfView.viewRadius, Color.yellow);
        RequestDecision();

        if (fieldOfView.visibleTargets.Count > 0)
        {
            // TODO

            isTargetVisible = true;
            target = fieldOfView.visibleTargets[0];
            lastKnownTargetPosition = target.position;
        }
        else if (fieldOfView.visibleTargets.Count == 0)
        {
            isTargetVisible = false;
            target = null;
        }

    }

    public void _AddReward()
    {
        throw new NotImplementedException();
    }
}

*/