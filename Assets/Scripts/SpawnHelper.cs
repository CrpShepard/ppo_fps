using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class SpawnHelper
{
    public static void SpawnInRandomPosition(ITarget target)
    {
        Transform transform = target.GetGameObject().transform;

        Vector3 dangerSpawn = Vector3.zero;
        Vector3 safeSpawn = Vector3.zero;

        Vector3 potentialPosition = Vector3.zero;
        Quaternion potentialRotation = new Quaternion();

        float maxDistance = 0f;

        for (int i = 0; i < 10; i++) {
            bool safePositionFound = false;
            int attemptsRemainig = 100; // избегаем вечного цикла
            // цикл, в котором ищем позицию для спавна
            while (!safePositionFound && attemptsRemainig > 0)
            {
                attemptsRemainig--;

                float x = UnityEngine.Random.Range(-49, 49) + UnityEngine.Random.Range(-1, 1) / 2;
                float z = UnityEngine.Random.Range(-49, 49) + UnityEngine.Random.Range(-1, 1) / 2;

                potentialPosition = new Vector3(x, 1, z);

                Collider[] colliders = Physics.OverlapSphere(potentialPosition, 0.5f);
                safePositionFound = colliders.Length == 0;
            }

            if (attemptsRemainig == 0) Debug.Assert(safePositionFound, "Невозможно найти место для спавна!");
            else
            {
                Collider[] colliders = Physics.OverlapSphere(potentialPosition, 15f);
                bool isSafe = true;
                
                foreach (Collider collider in colliders)
                {
                    if (collider.transform.parent.TryGetComponent<ITarget>(out ITarget someone) && target.IsEnemy(someone)) 
                    { 
                        // если в пределах 15 метрах есть враг, то кладем в список сверху самую дальнюю от него точку
                        isSafe = false;
                        float distance = Vector3.Distance(transform.position, collider.transform.position);
                        if (distance > maxDistance)
                        {
                            maxDistance = distance;
                            dangerSpawn = potentialPosition;
                        }
                    }
                }
                if (isSafe && safeSpawn == Vector3.zero) { safeSpawn = potentialPosition; break; } 
            }
        }

        // сам спавн
        float yaw = UnityEngine.Random.Range(-180f, 180f);
        potentialRotation = Quaternion.Euler(0f, yaw, 0f);

        if (safeSpawn != Vector3.zero) { transform.SetPositionAndRotation(safeSpawn, potentialRotation); }
        else if (dangerSpawn != Vector3.zero) { transform.SetPositionAndRotation(dangerSpawn, potentialRotation); }
        else { Debug.Assert(false, "Нет точек для спавна!"); }
    }

    public static void ResetState(ITarget target)
    {
        target.currentHealth = target.maxHealth;
        target.currentArmor = 0f;
        target.weapons.Clear();
        target.AddWeapon(target.GetGameObject().AddComponent<W_Crowbar>());
        target.ChangeWeapon(1);
        target.isDead = false;
    }
}
