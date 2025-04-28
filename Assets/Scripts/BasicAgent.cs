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
            // Реакция на получение урона (например, анимация)
        }
    }

    protected virtual void Die(ITeam source)
    {
        isDead = true;
        navAgent.isStopped = true;
        var rigidbody = GetComponent<Rigidbody>();
        rigidbody.Sleep();

        // Анимация смерти, выпадение лута и т.д.

        //Destroy(gameObject, 2f);
        Debug.Log(gameObject.name + " has died!");

        #pragma warning disable CS0252 // Возможно, использовано непреднамеренное сравнение ссылок: для левой стороны требуется приведение
        if (source != null && source != this) source.AddScore();
        #pragma warning restore CS0252 // Возможно, использовано непреднамеренное сравнение ссылок: для левой стороны требуется приведение
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

    // Спавним агента в случайном играбельном месте на уровне
    private void SpawnInRandomPosition()
    {
        bool safePositionFound = false;
        int attemptsRemainig = 100; // избегаем вечного цикла

        Vector3 potentialPosition = Vector3.zero;
        Quaternion potentialRotation = new Quaternion();

        // цикл, в котором ищем позицию для спавна
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

        Debug.Assert(safePositionFound, "Невозможно найти место для спавна!");

        transform.position = potentialPosition;
        transform.rotation = potentialRotation;
    }

    // Выбор цели (target) из нескольких противников
    private void UpdateNearestTarget()
    {
        // потом дописать когда целей будет больше 1
        //foreach (Transform target in )
    }

    // Если агент тронется триггера
    private void OnTriggerEnter(Collider other)
    {
        // аптечка, оружие, патроны
    }

    // Если агент стоит внутри триггер зоны
    private void OnTriggerStay(Collider other)
    {

    }

    // Если агент коснется чего-то (не триггер)
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
    // действия через OnActionReceived BEGIN
    public void GoalPosition(int goalType, int value1, int value2)
    {
        Vector3 goal = Vector3.zero;
        switch (goalType)
        {
            // стоять на месте
            case 0:
                navAgent.isStopped = true;
                break;
            // Квадрат
            case 1:
                navAgent.isStopped = false;
                goal = new Vector3(value1, 0, value2);

                // узнаем нужную высоту выбранной точки
                float y = Terrain.activeTerrain.SampleHeight(goal) + Terrain.activeTerrain.transform.position.y;

                goal = new Vector3(goal.x, y, goal.z);
                navAgent.CalculatePath(goal, path);

                // если возможно дойти до точки
                if (path.status != NavMeshPathStatus.PathComplete)
                {
                    navAgent.destination = goal;
                }

                // добавить штраф, если агент пытается идти к квадрату, которой нет на карте

                break;
            // Точка интереса
            case 2:
                navAgent.isStopped = false;
                goal = LevelEnv.waypoints[math.clamp(value1, 0, LevelEnv.waypoints.Length - 1)][math.clamp(value2, 0, LevelEnv.waypoints[value1].Length - 1)];
                // добавить штраф, если агент выводит значение большее, чем размер массива по уровню

                navAgent.CalculatePath(goal, path);

                // если возможно дойти до точки
                if (path.status != NavMeshPathStatus.PathComplete)
                {
                    navAgent.destination = goal;
                }
                break;
            // Полярные координаты
            case 3:
                navAgent.isStopped = false;

                // добавить штраф, если агент долго не может сдвинуться с точки в этом кейсе

                break;
            // Идти к врагу
            // можно добавить логику обхода с фланга через доп. два значения value2 и value3
            case 4:
                if (isTargetVisible)
                {
                    navAgent.isStopped = false;
                    goal = target.position;

                    navAgent.CalculatePath(goal, path);

                    // если возможно дойти до точки
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

                    // если возможно дойти до точки
                    if (path.status != NavMeshPathStatus.PathInvalid)
                    {
                        navAgent.destination = goal;
                    }
                }
                // добавить штраф, если агент пытается идти к врагу, которого он не видит
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
        // добавить штраф, если агент просто так атакует + если старается атаковать пустым оружием
    }
   
    public void Reload(int value)
    {
        if (value == 1)
        {
            currentWeapon.Reload();
        }
        // добавить штраф, если агент просто так перезаряжается + если агент перезаряжается если в магазине еще осталось куча боезапаса
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
        // добавить штраф, если агент просто так меняет оружие
    }

    // действия через OnActionReceived END
    //===============================================================================================================================

    // ======================================================================
    // ML-Agents functions BEGIN
    public override void Initialize()
    {
        var rigidbody = GetComponent<Rigidbody>();


        // нет ограничения по длительности эпизода вне тренировочного режима
        if (!isTrainingMode) MaxStep = 0;


    }

    // Поступили аргументы от нейронной сети для действий
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (isDead) return;

        // выбор маршрута
        GoalPosition(actions.DiscreteActions[0], actions.DiscreteActions[1], actions.DiscreteActions[2]);

        // поворот агента
        RotateAgent(actions.DiscreteActions[3], actions.DiscreteActions[4]);

        // атака агента
        Attack(actions.DiscreteActions[5]);

        // перезарядка оружия
        Reload(actions.DiscreteActions[6]);

        // смена оружия
        ChangeWeapon(actions.DiscreteActions[7]);
    }

    // Прокидываем нужные параметры агента и переменные в нейронную сеть
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

    // Если Behavior Type поставлен на "Heuristic Only", то будет вызываться
    // данная функция и значения будут высылаться в функцию OnActionReceived вместо нейронной сети
    // !!! для rule-based ИИ или для игрока будет потом сделано
    public override void Heuristic(in ActionBuffers actionsOut)
    {

    }

    // Старт эпизода
    public override void OnEpisodeBegin()
    {
        if (isTrainingMode)
        {
            // заново спавнить врагов, предметы и тд
        }

        score = 0;
        death = 0;

        var rigidbody = GetComponent<Rigidbody>();
        rigidbody.linearVelocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;

        // случайное место появления
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

        // Настройка NavMeshAgent
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
