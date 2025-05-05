using UnityEngine;

public class W_Crowbar : Weapon
{
    public new float damage = 25f;
    public new float fireRate = 150f;
    
    

    public override void Attack(Transform target = null)
    {
        //FireProjectile();
    }

    protected override void FireProjectile(Vector3 direction)
    {
        throw new System.NotImplementedException();
    }


}
