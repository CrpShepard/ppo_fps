using System.Collections;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum ItemType
    {
        Weapon = 0,
        AmmoPack = 1,
        HealthPack = 2,
        ArmorPack = 3
    }

    public enum WeaponType
    {
        Pistol = 2,
        Rifle = 3,
        Grenade = 4
    }

    public enum AmmoType
    {
        Pistol = 2,
        Rifle = 3
    }

    public ItemType type;

    [HideInInspector]
    public WeaponType weaponType;

    [HideInInspector]
    public AmmoType ammoType;

    float respawnTime;

    [HideInInspector]
    public GameObject prefabItem;

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.transform.parent.TryGetComponent<ITarget>(out ITarget target))
        {
            switch (type)
            {
                case ItemType.Weapon:
                    switch (weaponType)
                    {
                        case WeaponType.Pistol:
                            if (target.AddWeapon(target.GetGameObject().AddComponent<W_Pistol>())) { CoroutineRunner.Instance.StartCoroutine(Respawn()); gameObject.SetActive(false);  };
                            break;
                        case WeaponType.Rifle:
                            if (target.AddWeapon(target.GetGameObject().AddComponent<W_Rifle>())) { CoroutineRunner.Instance.StartCoroutine(Respawn()); gameObject.SetActive(false); };
                            break;
                        case WeaponType.Grenade:
                            if (target.AddWeapon(target.GetGameObject().AddComponent<W_Grenade>())) { CoroutineRunner.Instance.StartCoroutine(Respawn()); gameObject.SetActive(false); };
                            break;
                    }
                    break;
                case ItemType.AmmoPack:
                    switch (ammoType)
                    {
                        case AmmoType.Pistol:
                            if (target.AddAmmo(ammoType)) { CoroutineRunner.Instance.StartCoroutine(Respawn()); gameObject.SetActive(false); }
                            break;
                        case AmmoType.Rifle:
                            if (target.AddAmmo(ammoType)) { CoroutineRunner.Instance.StartCoroutine(Respawn()); gameObject.SetActive(false); }
                            break;
                    }
                    break;
                case ItemType.HealthPack:
                    if (target.AddHealth()) { CoroutineRunner.Instance.StartCoroutine(Respawn()); gameObject.SetActive(false); }
                    break;
                case ItemType.ArmorPack:
                    if (target.AddArmor()) { CoroutineRunner.Instance.StartCoroutine(Respawn()); gameObject.SetActive(false); }
                    break;
            }
        }
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTime);
        gameObject.SetActive(true);
    }

    void InitPrefabMesh()
    {
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }

        foreach (Transform child in prefabItem.transform)
        {
            MeshFilter childMeshFilter = child.GetComponent<MeshFilter>();

            if (childMeshFilter != null)
            {
                // Создаём новый дочерний объект
                GameObject newChild = new GameObject(child.name);
                newChild.transform.SetParent(transform);
                newChild.transform.localPosition = child.localPosition;
                newChild.transform.localRotation = child.localRotation;
                newChild.transform.localScale = child.localScale;

                // Копируем MeshFilter и MeshRenderer
                MeshFilter newMeshFilter = newChild.AddComponent<MeshFilter>();
                newMeshFilter.sharedMesh = childMeshFilter.sharedMesh;

                MeshRenderer newRenderer = newChild.AddComponent<MeshRenderer>();
                MeshRenderer originalRenderer = child.GetComponent<MeshRenderer>();
                if (originalRenderer != null)
                {
                    newRenderer.sharedMaterials = originalRenderer.sharedMaterials;
                }
            }
        }
    }

    public void InitItem()
    {
        switch (type)
        {
            case ItemType.Weapon:
                respawnTime = 30f;
                switch (weaponType)
                {
                    case WeaponType.Pistol:
                        prefabItem = Resources.Load<GameObject>("Prefabs/Pistol");
                        InitPrefabMesh();
                        break;
                    case WeaponType.Rifle:
                        prefabItem = Resources.Load<GameObject>("Prefabs/Rifle");
                        InitPrefabMesh();
                        break;
                    case WeaponType.Grenade:
                        prefabItem = Resources.Load<GameObject>("Prefabs/Grenade");
                        InitPrefabMesh();
                        break;
                }
                break;
            case ItemType.AmmoPack:
                respawnTime = 20f;
                switch (ammoType)
                {
                    case AmmoType.Pistol:
                        prefabItem = Resources.Load<GameObject>("Prefabs/AmmoPackPistol");
                        InitPrefabMesh();
                        break;
                    case AmmoType.Rifle:
                        prefabItem = Resources.Load<GameObject>("Prefabs/AmmoPackRifle");
                        InitPrefabMesh();
                        break;
                }
                break;
            case ItemType.HealthPack:
                respawnTime = 20f;
                prefabItem = Resources.Load<GameObject>("Prefabs/HealthPack");
                InitPrefabMesh();
                break;
            case ItemType.ArmorPack:
                respawnTime = 20f;
                prefabItem = Resources.Load<GameObject>("Prefabs/ArmorPack");
                InitPrefabMesh();
                break;
        }
    }

    void Start()
    {
        InitItem();
        //respawnTime = 3f;
    }
}
