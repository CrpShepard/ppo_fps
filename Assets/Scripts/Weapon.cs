using System.Collections;
using UnityEngine;

abstract public class Weapon : MonoBehaviour
{
    public float damage;
    public float fireRate;
    public float reloadTime;
    public int maxAmmo;
    public int currentAmmo;
    public float spreadAngle; // ������� ����

    protected bool isReloading = false;

    private void Start()
    {
        currentAmmo = maxAmmo;
    }

    public virtual void Attack(Transform target = null)
    {
        if (isReloading || currentAmmo <= 0) return;

        currentAmmo--;

        // ������ �������� (Raycast / Projectile)
        if (target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            direction = ApplySpread(direction); // ��������� �������
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