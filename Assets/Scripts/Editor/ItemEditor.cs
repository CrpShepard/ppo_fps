using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System;
using Unity.VisualScripting;

[CustomEditor(typeof(Item))]
public class ItemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Item item = (Item)target;

        if (item.type == Item.ItemType.Weapon)
        {
            item.weaponType = (Item.WeaponType)EditorGUILayout.EnumPopup("Weapon Type", item.weaponType);

            if (Enum.IsDefined(typeof(Item.WeaponType), item.weaponType))
            {
                item.InitItem();
            }

            if (item.prefabItem != null)
            {
                EditorUtility.SetDirty(item);
            }
        }

        if (item.type == Item.ItemType.AmmoPack)
        {
            item.ammoType = (Item.AmmoType)EditorGUILayout.EnumPopup("Ammo Type", item.ammoType);

            if (Enum.IsDefined(typeof(Item.AmmoType), item.ammoType))
            {
                item.InitItem();
            }

            if (item.prefabItem != null)
            {
                EditorUtility.SetDirty(item);
            }
        }

        if (item.type == Item.ItemType.HealthPack)
        {
            item.InitItem();

            if (item.prefabItem != null)
            {
                EditorUtility.SetDirty(item);
            }
        }

        if (item.type == Item.ItemType.ArmorPack)
        {
            item.InitItem();

            if (item.prefabItem != null)
            {
                EditorUtility.SetDirty(item);
            }
        }
    }

}
