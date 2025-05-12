using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.PlayerSettings;

public struct LastKnownTargetTransform
{
    public Vector3 position;
    public Quaternion rotation;
}

public class RuleBasedEnemy : MonoBehaviour, ITarget
{
    [Header("Scoreboard")]
    [SerializeField] protected byte team;
    [SerializeField] protected int score;
    [SerializeField] protected int death;

    [Header("Health")]
    public float currentHealth;
    [SerializeField] protected float maxHealth = 100f;

    [Header("Movement")]
    [SerializeField] protected float speed;
    [SerializeField] protected float rotationSpeed;
    protected float smoothYawChange = 0f;
    protected float smoothPitchChange = 0f;
    [SerializeField] protected float maxPitchAngle = 80f;

    // Components
    LevelEnv levelEnv;
    new Rigidbody rigidbody;
    Transform pointOfView;
    protected Transform target;
    FieldOfView fieldOfView;
    Vector3 fovPos;
    Vector3 fovForward;
    public EnemyLineOfSightChecker LineOfSightChecker;

    // Navigation
    Vector3 walkPoint;
    bool walkPointSet;
    //protected Vector3 lastKnownTargetPosition;
    public LastKnownTargetTransform lastKnownTargetTransform;
    protected NavMeshAgent navAgent;
    float stoppingDistance = 0.5f;
    public LayerMask whatIsGround, whatIsTarget, whatIsObstacle;
    [Range(-1, 1)]
    [Tooltip("Lower is a better hiding spot")]
    public float HideSensitivity = 0;
    [Range(1, 10)]
    public float MinPlayerDistance = 5f;
    [Range(0, 5f)]
    public float MinObstacleHeight = 1.25f;
    [Range(0.01f, 1f)]
    public float UpdateFrequency = 0.25f;
    private Collider[] Colliders = new Collider[10];

    [Header("Combat")]
    protected List<Weapon> weapons;
    [SerializeField] protected Weapon currentWeapon;

    [Header("States")]
    public bool isDead = false;
    protected bool isAttacking = false;
    protected bool isTargetVisible = false;
    bool isChasingEnemy = false;
    bool isSearchingEnemy = false;
    bool isSearchingEnemyPosSet = false;
    float timeSinceLastVisibleTarget = 0f; // при погоне и поиске
    float timeSinceLastVisibleTargetMax = 15f;
    float timeSinceEnemyWasSeen = 0f; // чтобы не обрывать бой, когда враг мог уйти в укрытие на очень короткий промежуток времени
    float timeSinceEnemyWasSeenMax = 4.5f;
    public float maxSeeDistance;
    public float coverSearchRadius;
    //public Vector3 coverPosition;
    public bool isTakingCover = false;
    public Coroutine hidingCoroutine;

    // дл€ отладки
    [Header("BehaviourParams")]
    [SerializeField] bool canMove = true;
    [SerializeField] bool canAttack = true;
    [SerializeField] bool canPickupItems = true;
    [SerializeField] bool canPickupWeapons = true;
    [SerializeField] bool isInvisible = false;
    [SerializeField] bool godMode = false;
    [SerializeField] int enemySearchRadius = 20;

    byte ITarget.team { get => team; set => team = value; }
    int ITarget.score { get => score; set => score = value; }
    int ITarget.death { get => death; }
    void ITarget.AddScore() { score++; }
    bool ITarget.IsEnemy(ITarget target)
    {
        if (target.team == team) return false;
        return true;
    }

    void ITarget.TakeDamage(float damage, ITarget source) { TakeDamage(damage, source); }

    bool ITarget.isDead { get => isDead; }

    enum WeaponPriority
    {
        Melee = 0,
        Pistol = 1,
        Rifle = 2,
        Grenade = -1
    }

    void LastKnownTargetTransform(Vector3 position, Quaternion rotation)
    {
        lastKnownTargetTransform.position = position;
        lastKnownTargetTransform.rotation = rotation;
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
        return transform.position;
    }

    bool IsWalkPointReached()
    {
        if (!navAgent.pathPending)
        {
            if (navAgent.remainingDistance <= navAgent.stoppingDistance)
            {
                if (!navAgent.hasPath || navAgent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    void ChangeLayerMask(string layerMask)
    {
        gameObject.layer = LayerMask.NameToLayer(layerMask);
        foreach (Transform child in transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer(layerMask);
        }
    }

    protected virtual void Die(ITarget source)
    {
        isDead = true;
        navAgent.isStopped = true;
        ChangeLayerMask("Ignore Raycast");
        rigidbody.Sleep();

        // јнимаци€ смерти, выпадение лута и т.д.
        Debug.Log(gameObject.name + " has died!");

#pragma warning disable CS0252 // ¬озможно, использовано непреднамеренное сравнение ссылок: дл€ левой стороны требуетс€ приведение
        if (source != null && source != this) source.AddScore();
#pragma warning restore CS0252 // ¬озможно, использовано непреднамеренное сравнение ссылок: дл€ левой стороны требуетс€ приведение
        
        // TODO добавить respawn
    }

    public virtual void TakeDamage(float damage, ITarget source)
    {
        if (!godMode)
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die(source);
        }
        else
        {
            // –еакци€ на получение урона (например, анимаци€)
        }
    }

    public virtual void GetHealth(float health)
    {
        currentHealth += health;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    protected void HitEnemy(ITarget target)
    {
        target.TakeDamage(currentWeapon.damage, this);
    }

    public void AddWeapon(Weapon weapon)
    {
        bool alreadyExists = weapons.Any(w => w.GetType() == weapon.GetType());

        if (!alreadyExists)
        {
            weapons.Add(weapon);
        }
    }

    public void AttackTarget()
    {
        
        //transform.LookAt(target.position);

        Vector3 dirToTarget = (target.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(dirToTarget);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // если прицел попал в область атаки
        if (Physics.SphereCast(fovPos, 0.25f, fovForward, out RaycastHit hit, maxSeeDistance, whatIsTarget))
        //if (Physics.Raycast(fovPos, fovForward, out RaycastHit hit, maxSeeDistance, whatIsTarget))
        {
            //Debug.Log("AttackTarget()");
            Attack();
        }

    }

    public void Attack()
    {
        if (canAttack)
        {
            currentWeapon.Attack(out ITarget targetAttack, fovPos, fovForward);
            if (targetAttack != null)
            {
                Debug.Log("targetAttack != null");
                HitEnemy(targetAttack);
            }
        }
    }

    public void Reload()
    {
        if (currentWeapon.GetType() != typeof(W_Crowbar))
        {
            //if (isTargetVisible && currentWeapon.currentAmmo / currentWeapon.maxAmmo > 0.33f) AddReward(-0.2f);
            currentWeapon.Reload();
        }

        
    }

    public void ChangeWeapon(int value)
    {
        switch (value)
        {
            // ћонтировка
            case 1:
                Weapon neededWeapon = weapons.OfType<W_Crowbar>().FirstOrDefault();
                if (neededWeapon && currentWeapon != neededWeapon)
                {
                    currentWeapon = neededWeapon;
                    // TODO добавить задержку перед применением
                }
                break;
        }

        // добавить штраф, если агент просто так мен€ет оружие
    }

    void UpdateFovVisibleTarget()
    {
        if (fieldOfView.visibleTargets.Count > 0)
        {
            // TODO

            isTargetVisible = true;
            target = fieldOfView.visibleTargets[0];
            //lastKnownTargetPosition = target.position;
            LastKnownTargetTransform(target.position, target.rotation);
            timeSinceEnemyWasSeen = 0f;
        }
        else if (fieldOfView.visibleTargets.Count == 0)
        {
            if (isTargetVisible && timeSinceEnemyWasSeen < timeSinceEnemyWasSeenMax) 
            {
                timeSinceEnemyWasSeen += Time.deltaTime;
                //target.position = lastKnownTargetPosition;
                return;
            }
            
            if (isTargetVisible && !isTakingCover) isChasingEnemy = true;
            isTargetVisible = false;
            LineOfSightChecker.EndCheckForLineOfSightCoroutine(lastKnownTargetTransform);
            isAttacking = false;
            target = null;
        }
    }

    // ============================
    // —истема укрыти€ BEGIN

    //private void HandleGainSight(Transform Target)
    // ≈сли видно врага
    private void HandleGainSight(LastKnownTargetTransform Target)
    {
        if (hidingCoroutine != null)
        {
            StopCoroutine(hidingCoroutine);
        }
        //Player = Target;
        if (isTakingCover)
            hidingCoroutine = StartCoroutine(Hide(Target));
    }

    //private void HandleLoseSight(Transform Target)
    // ≈сли врага не видно
    private void HandleLoseSight(LastKnownTargetTransform Target)
    {
        if (hidingCoroutine != null)
        {
            StopCoroutine(hidingCoroutine);
        }
        //Player = null;
    }

    //private IEnumerator Hide(Transform Target)
    private IEnumerator Hide(LastKnownTargetTransform Target)
    {
        WaitForSeconds Wait = new WaitForSeconds(UpdateFrequency);
        while (true)
        {
            for (int i = 0; i < Colliders.Length; i++)
            {
                Colliders[i] = null;
            }

            int hits = Physics.OverlapSphereNonAlloc(transform.position, LineOfSightChecker.Collider.radius, Colliders, whatIsObstacle);

            int hitReduction = 0;
            for (int i = 0; i < hits; i++)
            {
                if (Vector3.Distance(Colliders[i].transform.position, Target.position) < MinPlayerDistance || Colliders[i].bounds.size.y < MinObstacleHeight)
                {
                    Colliders[i] = null;
                    hitReduction++;
                }
            }
            hits -= hitReduction;

            System.Array.Sort(Colliders, ColliderArraySortComparer);

            for (int i = 0; i < hits; i++)
            {
                if (NavMesh.SamplePosition(Colliders[i].transform.position, out NavMeshHit hit, 2f, navAgent.areaMask))
                {
                    if (!NavMesh.FindClosestEdge(hit.position, out hit, navAgent.areaMask))
                    {
                        Debug.LogError($"Unable to find edge close to {hit.position}");
                    }

                    if (Vector3.Dot(hit.normal, (Target.position - hit.position).normalized) < HideSensitivity)
                    {
                        navAgent.SetDestination(hit.position);
                        break;
                    }
                    else
                    {
                        // Since the previous spot wasn't facing "away" enough from teh target, we'll try on the other side of the object
                        if (NavMesh.SamplePosition(Colliders[i].transform.position - (Target.position - hit.position).normalized * 2, out NavMeshHit hit2, 2f, navAgent.areaMask))
                        {
                            if (!NavMesh.FindClosestEdge(hit2.position, out hit2, navAgent.areaMask))
                            {
                                Debug.LogError($"Unable to find edge close to {hit2.position} (second attempt)");
                            }

                            if (Vector3.Dot(hit2.normal, (Target.position - hit2.position).normalized) < HideSensitivity)
                            {
                                navAgent.SetDestination(hit2.position);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError($"Unable to find NavMesh near object {Colliders[i].name} at {Colliders[i].transform.position}");
                }
            }
            yield return Wait;
        }
    }

    public int ColliderArraySortComparer(Collider A, Collider B)
    {
        if (A == null && B != null)
        {
            return 1;
        }
        else if (A != null && B == null)
        {
            return -1;
        }
        else if (A == null && B == null)
        {
            return 0;
        }
        else
        {
            return Vector3.Distance(transform.position, A.transform.position).CompareTo(Vector3.Distance(transform.position, B.transform.position));
        }
    }

    // —истема укрыти€ END
    // ============================

    void Awake()
    {
        levelEnv = GetComponentInParent<LevelEnv>();
        rigidbody = GetComponent<Rigidbody>();

        navAgent = GetComponent<NavMeshAgent>();
        navAgent.speed = speed;
        navAgent.angularSpeed = rotationSpeed;
        navAgent.stoppingDistance = stoppingDistance;

        pointOfView = this.transform.Find("PointOfView");
        fieldOfView = pointOfView.GetComponent<FieldOfView>();

        weapons = new List<Weapon>() { gameObject.AddComponent<W_Crowbar>() };
        currentWeapon = weapons[0];

        currentHealth = maxHealth;
        walkPoint = transform.position;

        LineOfSightChecker = GetComponent<EnemyLineOfSightChecker>();
        LineOfSightChecker.OnGainSight += HandleGainSight;
        LineOfSightChecker.OnLoseSight += HandleLoseSight;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;
        if (!canMove) { navAgent.speed = 0f; navAgent.angularSpeed = 0f; }
        if (canMove) { navAgent.speed = speed; navAgent.angularSpeed = rotationSpeed; }
        if (!isInvisible) ChangeLayerMask("Target");
        if (isInvisible) ChangeLayerMask("Ignore Raycast");

        fovPos = fieldOfView.transform.position;
        fovForward = Quaternion.Inverse(fieldOfView.transform.localRotation) * Vector3.forward;

        Debug.DrawRay(fovPos, fovForward * fieldOfView.viewRadius, Color.yellow);

        UpdateFovVisibleTarget();

        if (isTargetVisible)
        {
            if (currentHealth > 40f)
                navAgent.destination = target.position;
            else
                isTakingCover = true;
            isChasingEnemy = false;
            isSearchingEnemy = false;
            isSearchingEnemyPosSet = false;
            timeSinceLastVisibleTarget = 0f;

            if (currentWeapon.attackDistance >= Vector3.Distance(transform.position, target.position))
            {
                isAttacking = true;
                if (!isTakingCover)
                    navAgent.destination = transform.position;
                AttackTarget();
            }
            else
            {
                isAttacking = false;
            }

            LineOfSightChecker.StartCheckForLineOfSightCoroutine(lastKnownTargetTransform);
        }

        if (isChasingEnemy)
        {
            //navAgent.destination = lastKnownTargetPosition;
            navAgent.destination = lastKnownTargetTransform.position;

            if (IsWalkPointReached())
            {
                isChasingEnemy = false;
                isSearchingEnemy = true;
                isSearchingEnemyPosSet = false;
            }

            if (currentWeapon.magCurrentAmmo / currentWeapon.magMaxAmmo <= 0.33f && currentWeapon.currentAmmo > 0) Reload();
        }
        else if (isSearchingEnemy)
        {
            if (!isSearchingEnemyPosSet)
            {
                //float x = lastKnownTargetPosition.x + UnityEngine.Random.Range(-enemySearchRadius, enemySearchRadius) + UnityEngine.Random.Range(-1, 1) / 2;
                //float z = lastKnownTargetPosition.z + UnityEngine.Random.Range(-enemySearchRadius, enemySearchRadius) + UnityEngine.Random.Range(-1, 1) / 2;
                float x = lastKnownTargetTransform.position.x + UnityEngine.Random.Range(-enemySearchRadius, enemySearchRadius) + UnityEngine.Random.Range(-1, 1) / 2;
                float z = lastKnownTargetTransform.position.z + UnityEngine.Random.Range(-enemySearchRadius, enemySearchRadius) + UnityEngine.Random.Range(-1, 1) / 2;
                navAgent.destination = new Vector3(x, 0, z);
                isSearchingEnemyPosSet = true;
            }
            if (IsWalkPointReached())
            {
                isSearchingEnemyPosSet = false;
            }

            if (currentWeapon.magCurrentAmmo / currentWeapon.magMaxAmmo <= 0.66f && currentWeapon.currentAmmo > 0) Reload();
        }

        if (timeSinceLastVisibleTarget >= timeSinceLastVisibleTargetMax)
        {
            isChasingEnemy = false;
            isSearchingEnemy = false;
            isSearchingEnemyPosSet = false;
            timeSinceLastVisibleTarget = 0f;
        }
        if (isChasingEnemy || isSearchingEnemy)
        {
            timeSinceLastVisibleTarget += Time.deltaTime;
        }

        if (!isTargetVisible && !isChasingEnemy && !isSearchingEnemy)
        {
            if (IsWalkPointReached())
            {
                walkPoint = RandomWalkPoint();
                navAgent.destination = walkPoint;
            }

            if (currentWeapon.magCurrentAmmo < currentWeapon.magMaxAmmo && currentWeapon.currentAmmo > 0)
            {
                Reload();
            }
        }
    }
}
