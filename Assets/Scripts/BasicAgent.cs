using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Mathematics;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.AI;

public class BasicAgent : Agent, ITeam
{
    [Header("Scoreboard")]
    [SerializeField] protected byte team;
    [SerializeField] protected int score;
    [SerializeField] protected int death;

    [Header("Health")]
    public float currentHealth { get => currentHealth; protected set => currentHealth = value; }
    [SerializeField] protected float maxHealth = 100f;

    [Header("Movement")]
    new Rigidbody rigidbody;
    Camera viewCamera;
    [SerializeField] protected float speed;
    //[SerializeField] protected float currentSpeed;
    //[SerializeField] protected float maxSpeed;
    [SerializeField] protected float rotationSpeed;
    protected float smoothYawChange = 0f;
    protected float smoothPitchChange = 0f;
    [SerializeField] protected float stoppingDistance;
    [SerializeField] protected float maxPitchAngle = 80f;

    

    [Header("Combat")]
    protected List<Weapon> weapons;
    [SerializeField] protected Weapon currentWeapon;

    protected float maxSeeDistance;
    protected Transform target;
    protected Vector3 lastKnownTargetPosition;
    protected NavMeshAgent navAgent;
    protected NavMeshPath path;
    //protected Animator animator;
    protected Collider enemyCollider;

    [Header("States")]
    [SerializeField] protected bool isTrainingMode;
    protected bool isDead = false;
    protected bool isAttacking = false;
    protected bool isTargetVisible = false;

    byte ITeam.team {
        get => team;
        set => team = value;
    }

    int ITeam.score { get => score; set => score = value; }
    int ITeam.death { get => death; }

    bool ITeam.IsEnemy(ITeam target)
    {
        if (target.team == team) return false;
        else return true;
    }

    void ITeam.AddScore()
    {
        score++;
    }

    public virtual void GetHealth(float health)
    {
        currentHealth += health;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    public virtual void TakeDamage(float damage, ITeam source)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die(source);
        }
        else
        {
            // ������� �� ��������� ����� (��������, ��������)
        }
    }

    protected virtual void Die(ITeam source)
    {
        isDead = true;
        navAgent.isStopped = true;
        var rigidbody = GetComponent<Rigidbody>();
        rigidbody.Sleep();

        // �������� ������, ��������� ���� � �.�.

        //Destroy(gameObject, 2f);
        Debug.Log(gameObject.name + " has died!");

        #pragma warning disable CS0252 // ��������, ������������ ���������������� ��������� ������: ��� ����� ������� ��������� ����������
        if (source != null && source != this) source.AddScore();
        #pragma warning restore CS0252 // ��������, ������������ ���������������� ��������� ������: ��� ����� ������� ��������� ����������
    }

    protected virtual bool CanSeeTarget()
    {
        RaycastHit hit;
        Vector3 directionToPlayer = (target.position - transform.position).normalized;

        if (Physics.Raycast(origin: transform.position, direction: directionToPlayer, out hit, maxDistance: maxSeeDistance))
        {
            return hit.transform == target;
        }
        return false;
    }

    protected void HitEnemy(Transform target)
    {
        BasicAgent enemy = target.gameObject.GetComponent<BasicAgent>();
        enemy.TakeDamage(currentWeapon.damage, this);
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

            float radius = UnityEngine.Random.Range(2f, 20f);
            Quaternion direction = Quaternion.Euler(0f, UnityEngine.Random.Range(-180f, 180f), 0f);
            potentialPosition = direction * Vector3.forward * radius;

            float yaw = UnityEngine.Random.Range(-180f, 180f);
            potentialRotation = Quaternion.Euler(0f, yaw, 0f);

            Collider[] colliders = Physics.OverlapSphere(potentialPosition, 0.25f);
            safePositionFound = colliders.Length == 0;
        }

        Debug.Assert(safePositionFound, "���������� ����� ����� ��� ������!");

        transform.position = potentialPosition;
        transform.rotation = potentialRotation;
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
        }
    }

    // ============================================================================================================
    // �������� ����� OnActionReceived BEGIN
    public void GoalPosition(int goalType, int value1, int value2)
    {
        Vector3 goal = Vector3.zero;
        switch (goalType)
        {
            // ������ �� �����
            case 0:
                navAgent.isStopped = true;
                break;
            // �������
            case 1:
                navAgent.isStopped = false;
                goal = new Vector3(value1, 0, value2);

                // ������ ������ ������ ��������� �����
                float y = Terrain.activeTerrain.SampleHeight(goal) + Terrain.activeTerrain.transform.position.y;

                goal = new Vector3(goal.x, y, goal.z);
                navAgent.CalculatePath(goal, path);

                // ���� �������� ����� �� �����
                if (path.status != NavMeshPathStatus.PathComplete)
                {
                    navAgent.destination = goal;
                }

                // �������� �����, ���� ����� �������� ���� � ��������, ������� ��� �� �����

                break;
            // ����� ��������
            case 2:
                navAgent.isStopped = false;
                goal = LevelEnv.waypoints[math.clamp(value1, 0, LevelEnv.waypoints.Length - 1)][math.clamp(value2, 0, LevelEnv.waypoints[value1].Length - 1)];
                // �������� �����, ���� ����� ������� �������� �������, ��� ������ ������� �� ������

                navAgent.CalculatePath(goal, path);

                // ���� �������� ����� �� �����
                if (path.status != NavMeshPathStatus.PathComplete)
                {
                    navAgent.destination = goal;
                }
                break;
            // �������� ����������
            case 3:
                navAgent.isStopped = false;

                // �������� �����, ���� ����� ����� �� ����� ���������� � ����� � ���� �����

                break;
            // ���� � �����
            // ����� �������� ������ ������ � ������ ����� ���. ��� �������� value2 � value3
            case 4:
                if (isTargetVisible)
                {
                    navAgent.isStopped = false;
                    goal = target.position;

                    navAgent.CalculatePath(goal, path);

                    // ���� �������� ����� �� �����
                    if (path.status != NavMeshPathStatus.PathInvalid)
                    {
                        navAgent.destination = goal;
                    }
                }
                else if (!isTargetVisible && lastKnownTargetPosition != Vector3.zero)
                {
                    navAgent.isStopped = false;
                    goal = lastKnownTargetPosition;

                    navAgent.CalculatePath(goal, path);

                    // ���� �������� ����� �� �����
                    if (path.status != NavMeshPathStatus.PathInvalid)
                    {
                        navAgent.destination = goal;
                    }
                }
                // �������� �����, ���� ����� �������� ���� � �����, �������� �� �� �����
                break;
        }
    }

    public void RotateAgent(int value1, int value2)
    {
        Vector3 rotationVector = transform.rotation.eulerAngles;

        float pitchChange = value1;
        smoothPitchChange = Mathf.MoveTowards(smoothPitchChange, pitchChange, 2f * Time.fixedDeltaTime);
        float pitch = rotationVector.x + smoothPitchChange * Time.fixedDeltaTime * rotationSpeed;
        if (pitch > 180f) pitch -= 360f;
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        float yawChange = value2;
        smoothYawChange = Mathf.MoveTowards(smoothYawChange, yawChange, 2f * Time.fixedDeltaTime);
        float yaw = rotationVector.y + smoothYawChange * Time.fixedDeltaTime * rotationSpeed;

        transform.rotation = Quaternion.Euler(pitch, yaw, 0);
    }

    public void Attack(int value)
    {
        if (value == 1)
        {
            currentWeapon.Attack();
        }
        // �������� �����, ���� ����� ������ ��� ������� + ���� ��������� ��������� ������ �������
    }
   
    public void Reload(int value)
    {
        if (value == 1)
        {
            currentWeapon.Reload();
        }
        // �������� �����, ���� ����� ������ ��� �������������� + ���� ����� �������������� ���� � �������� ��� �������� ���� ���������
    }

    public void ChangeWeapon(int value)
    {
        if (value == 1)
        {
            int index = weapons.IndexOf(currentWeapon);
            index++;

            if (index != 0 && index < weapons.Count)
            {
                currentWeapon = weapons[index];
            }
        }
        // �������� �����, ���� ����� ������ ��� ������ ������
    }

    // �������� ����� OnActionReceived END
    //===============================================================================================================================

    // ======================================================================
    // ML-Agents functions BEGIN
    public override void Initialize()
    {
        var rigidbody = GetComponent<Rigidbody>();


        // ��� ����������� �� ������������ ������� ��� �������������� ������
        if (!isTrainingMode) MaxStep = 0;


    }

    // ��������� ��������� �� ��������� ���� ��� ��������
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (isDead) return;

        // ����� ��������
        GoalPosition(actions.DiscreteActions[0], actions.DiscreteActions[1], actions.DiscreteActions[2]);

        // ������� ������
        RotateAgent(actions.DiscreteActions[3], actions.DiscreteActions[4]);

        // ����� ������
        Attack(actions.DiscreteActions[5]);

        // ����������� ������
        Reload(actions.DiscreteActions[6]);

        // ����� ������
        ChangeWeapon(actions.DiscreteActions[7]);
    }

    // ����������� ������ ��������� ������ � ���������� � ��������� ����
    public override void CollectObservations(VectorSensor sensor)
    {
       
        sensor.AddObservation(transform.localRotation.normalized);

        sensor.AddObservation(currentHealth);
        sensor.AddObservation(speed);
        sensor.AddObservation(currentWeapon);
        sensor.AddObservation(maxSeeDistance);
        sensor.AddObservation(target);
        sensor.AddObservation(lastKnownTargetPosition);
        sensor.AddObservation(isAttacking);
        sensor.AddObservation(isDead);
        sensor.AddObservation(isTargetVisible);
        sensor.AddObservation(team);

    }

    // ���� Behavior Type ��������� �� "Heuristic Only", �� ����� ����������
    // ������ ������� � �������� ����� ���������� � ������� OnActionReceived ������ ��������� ����
    // !!! ��� rule-based �� ��� ��� ������ ����� ����� �������
    public override void Heuristic(in ActionBuffers actionsOut)
    {

    }

    // ����� �������
    public override void OnEpisodeBegin()
    {
        if (isTrainingMode)
        {
            // ������ �������� ������, �������� � ��
        }

        score = 0;
        death = 0;

        var rigidbody = GetComponent<Rigidbody>();
        rigidbody.linearVelocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;

        // ��������� ����� ���������
        SpawnInRandomPosition();
    }

    // ML-Agents functions END
    // ======================================================================

    protected virtual void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.angularSpeed = 0;
        //animator = GetComponent<Animator>();
        currentHealth = maxHealth;

        // ��������� NavMeshAgent
        navAgent.speed = speed;
        navAgent.angularSpeed = rotationSpeed;
        navAgent.stoppingDistance = stoppingDistance;

        path = new NavMeshPath();
        //playerTarget = GameObject.FindGameObjectWithTag("Player").transform;

        currentWeapon = weapons[0];

        viewCamera = Camera.main;

        weapons = new List<Weapon>() { new W_Crowbar { } };
    }

    protected void Update()
    {
        if (isDead) return;

        
    }
}
