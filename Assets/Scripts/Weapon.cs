using System.Collections;
using UnityEngine;
using static UnityEditor.PlayerSettings;

abstract public class Weapon : MonoBehaviour
{
    // статы
    public Item.WeaponType WeaponType;
    public Item.AmmoType AmmoType;
    public float damage;
    public float fireRate;
    public float reloadTime;
    public int maxAmmo;
    public int magMaxAmmo;
    public float spreadAngle_base; // Разброс пуль базовый (в покое)
    public float spreadAngle_moving; // Разброс пуль при движении
    public float spreadAngle_firing; // Разброс пуль добавочный при каждом выстреле
    public float spreadAngle_max; // Разброс пуль макс. значение
    public float recoveryTime;
    public float equipTime;
    public float attackDistance;

    // компонент
    //public LayerMask targetLayer;
    //public LayerMask obstacleLayer;
    ParticleSystem MuzzleFlash;
    public Transform BulletSpawnPoint;
    public ParticleSystem ImpactParticle;
    TrailRenderer bulletTrail;
    public GameObject weaponPrefab;

    // меняющиеся
    public float timeSinceLastAttack;
    public int currentAmmo;
    public int magCurrentAmmo;
    public float spreadAngle_current;

    // флажки
    public bool isEquiping = false;
    public bool isHolderMoving;
    public bool isReloadable;
    public bool isReloading = false;
    public bool CanAttack() { 
        if (magCurrentAmmo > 0 && !isReloading && !isEquiping && timeSinceLastAttack > 1f / (fireRate / 60f)) return true;
        return false;
    }

    public virtual void Attack(ITarget source, Vector3 position, Vector3 dir)
    {
        if (magCurrentAmmo <= 0)
        {
            if (source is IAgent agent) { agent._AddReward(-0.01f); }
            // звук пустого оружия
        }

        if (!CanAttack()) return;

        magCurrentAmmo--;

        // Логика выстрела (Raycast)
        Vector3 direction = ApplySpread(dir); // Добавляем разброс

        Debug.DrawRay(position, direction * attackDistance, Color.magenta, 2f);
        if (Physics.Raycast(position, direction, out RaycastHit hit, attackDistance))
        {
            if (hit.transform.TryGetComponent<ITarget>(out ITarget enemy))
            {
                enemy.TakeDamage(damage, source);
                if (source is IAgent agent) { agent._AddReward(0.1f); }
            }
            else if (source is IAgent agent) { agent._AddReward(-0.01f); }

            TrailRenderer trail = Instantiate(bulletTrail, BulletSpawnPoint.position, Quaternion.identity);
            StartCoroutine(SpawnTrail(trail, hit));
        }

        ParticleSystem muzzleFlashPrefab = Instantiate(MuzzleFlash, BulletSpawnPoint.position, BulletSpawnPoint.rotation);

        Destroy(muzzleFlashPrefab.transform.GetChild(0).gameObject, 0.4f);
        Destroy(muzzleFlashPrefab.gameObject, 0.4f);

        timeSinceLastAttack = 0f;

        spreadAngle_current += spreadAngle_firing; // увеличиваем разброс для следующего выстрела
        if (spreadAngle_current > spreadAngle_max) spreadAngle_current = spreadAngle_max;
    }

    Vector3 ApplySpread(Vector3 direction)
    {
        float spreadX = Random.Range(-spreadAngle_current, spreadAngle_current);
        float spreadY = Random.Range(-spreadAngle_current, spreadAngle_current);
        return Quaternion.Euler(spreadX, spreadY, 0) * direction;
    }

    IEnumerator SpawnTrail(TrailRenderer Trail, RaycastHit Hit)
    {
        float time = 0f;
        Vector3 startPosition = Trail.transform.position;

        while (time < 1f)
        {
            Trail.transform.position = Vector3.Lerp(startPosition, Hit.point, time);
            time += Time.deltaTime / Trail.time;

            yield return null;
        }
        Trail.transform.position = Hit.point;
        Instantiate(ImpactParticle, Hit.point, Quaternion.LookRotation(Hit.normal));

        Destroy(Trail.gameObject, Trail.time);
    }

    public void Reload()
    {
        if (isReloadable && currentAmmo > 0 && magCurrentAmmo < magMaxAmmo && !isReloading && !isEquiping) { StartCoroutine(CReload()); }
    }

    public virtual IEnumerator CReload()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);

        int ammoNeeded = magMaxAmmo - magCurrentAmmo;
        int ammoToReload = Mathf.Min(ammoNeeded, currentAmmo);
        currentAmmo -= ammoToReload;
        magCurrentAmmo += ammoToReload;
        isReloading = false;
    }

    public void Equip(ITarget target)
    {
        StartCoroutine(CEquip(target));
    }

    public IEnumerator CEquip(ITarget target)
    {
        isEquiping = true;
        
        BulletSpawnPoint = target.EquipWeaponPrefab(weaponPrefab).transform.Find("MuzzleFlash");
        // звук экипировки оружия

        yield return new WaitForSeconds(equipTime);
        isEquiping = false;
    }

    public virtual bool CheckAmmo() { return (currentAmmo < maxAmmo) ? true : false; }
    
    public virtual void AddAmmo()
    {
        currentAmmo += magMaxAmmo;
        Mathf.Clamp(currentAmmo, 0, maxAmmo); 
    }

    public virtual void Awake()
    {
        currentAmmo = maxAmmo;
        magCurrentAmmo = magMaxAmmo;
    }

    private void Start()
    {
        MuzzleFlash = Resources.Load<ParticleSystem>("Prefabs/MuzzleFlash");
        ImpactParticle = Resources.Load<ParticleSystem>("Prefabs/HitEffect");
        bulletTrail = Resources.Load<TrailRenderer>("Prefabs/BulletTrail");
    }

    public virtual void Update()
    {
        timeSinceLastAttack += Time.deltaTime;
        /*
        if (timeSinceLastAttack >= recoveryTime)
        {
            if (!isHolderMoving) spreadAngle_current = spreadAngle_base;
            if (isHolderMoving) spreadAngle_current = spreadAngle_moving;
            
        }
        else // линейно уменьшаем разброс до минимума
        {
            if (!isHolderMoving) spreadAngle_current = Mathf.MoveTowards(spreadAngle_current, spreadAngle_base, recoveryTime * Time.deltaTime);
            if (isHolderMoving) spreadAngle_current = Mathf.MoveTowards(spreadAngle_current, spreadAngle_moving, recoveryTime * Time.deltaTime);
        }
        */

        if (!isHolderMoving) spreadAngle_current = Mathf.MoveTowards(spreadAngle_current, spreadAngle_base, recoveryTime * Time.deltaTime);
        if (isHolderMoving) spreadAngle_current = Mathf.MoveTowards(spreadAngle_current, spreadAngle_moving, recoveryTime * Time.deltaTime);
    }
}