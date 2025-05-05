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

    float timeSinceLastAttack;

    protected bool isReloading = false;
    bool CanAttack() => !isReloading && timeSinceLastAttack > 1f / (fireRate / 60f);

    private void Start()
    {
        currentAmmo = maxAmmo;
        magCurrentAmmo = magMaxAmmo;
    }

    public virtual void Attack(Transform target = null)
    {
        if (magCurrentAmmo <= 0) return;
        if (!CanAttack()) return;

        magCurrentAmmo--;

        // Логика выстрела (Raycast / Projectile)
        if (target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            direction = ApplySpread(direction); // Добавляем разброс
            FireProjectile(direction);
        }

        if (currentAmmo <= 0)
        {
            //StartCoroutine(Reload());
        }
    }

    protected virtual Vector3 ApplySpread(Vector3 direction)
    {
        float spreadX = Random.Range(-spreadAngle, spreadAngle);
        float spreadY = Random.Range(-spreadAngle, spreadAngle);
        return Quaternion.Euler(spreadX, spreadY, 0) * direction;
    }

    protected abstract void FireProjectile(Vector3 direction);

    public virtual IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        isReloading = false;
    }
}