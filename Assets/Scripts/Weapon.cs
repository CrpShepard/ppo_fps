using System.Collections;
using UnityEngine;

abstract public class Weapon : MonoBehaviour
{
    public float damage;
    public float fireRate;
    public float reloadTime;
    public int maxAmmo;
    public int currentAmmo;
    public int magCurrentAmmo;
    public int magMaxAmmo;
    public float spreadAngle; // Разброс пуль
    public float attackDistance;

    public float timeSinceLastAttack;

    public LayerMask targetLayer;

    bool isReloading = false;
    public bool CanAttack() { 
        if (!isReloading && timeSinceLastAttack > 1f / (fireRate / 60f)) return true;
        return false;
    }

    public virtual void Attack(out ITarget hitTarget, Vector3 pos, Vector3 dir)
    {
        hitTarget = null;
        if (magCurrentAmmo <= 0) return;
        if (!CanAttack()) return;

        magCurrentAmmo--;

        // Логика выстрела (Raycast)
        Vector3 direction = dir;
        direction = ApplySpread(direction); // Добавляем разброс
        
        Debug.DrawLine(pos, direction, Color.magenta, 5f);
        if (Physics.Raycast(pos, direction, out RaycastHit hit, attackDistance, targetLayer))
        {
            if (hit.transform.TryGetComponent<ITarget>(out ITarget enemy)) { hitTarget = enemy; }
        }

        if (currentAmmo <= 0)
        {
            //StartCoroutine(Reload());
        }
        timeSinceLastAttack = 0f;
    }

    protected virtual Vector3 ApplySpread(Vector3 direction)
    {
        float spreadX = Random.Range(-spreadAngle, spreadAngle);
        float spreadY = Random.Range(-spreadAngle, spreadAngle);
        return Quaternion.Euler(spreadX, spreadY, 0) * direction;
    }

    public virtual IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);

        int ammoNeeded = magMaxAmmo - magCurrentAmmo;
        int ammoToReload = Mathf.Min(ammoNeeded, currentAmmo);
        currentAmmo -= ammoToReload;
        magCurrentAmmo = ammoToReload;
        isReloading = false;
    }

    private void Awake()
    {
        currentAmmo = maxAmmo;
        magCurrentAmmo = magMaxAmmo;
    }

    private void Update()
    {
        timeSinceLastAttack += Time.deltaTime;
    }
}