using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class LevelEnv : MonoBehaviour
{
    // 0 - оружие, 1 - патроны, 2 - аптечки, 3 - брон€
    /*
    public List<Tuple<Vector3, Item.WeaponType>> weapons = new List<Tuple<Vector3, Item.WeaponType>>();
    public List<Tuple<Vector3, Item.AmmoType>> ammoPacks = new List<Tuple<Vector3, Item.AmmoType>>();
    public List<Vector3> healthPacks = new List<Vector3>();
    public List<Vector3> armorPacks = new List<Vector3>();
    */

    public List<Item> weapons = new List<Item>();
    public List<Item> ammoPacks = new List<Item>();
    public List<Item> healthPacks = new List<Item>();
    public List<Item> armorPacks = new List<Item>();

    public float cellSize = 0.5f;
    public Vector2 gridSize = new Vector2(100f, 100f);
    public bool[,] walkableGrid;

    float roundTime = 300f; // врем€ матча

    PlayerUI playerUI;

    // √енераци€ сетки поверхности, по которой можно ходить
    public void GenerateNavMeshGrid()
    {
        int gridWidth = Mathf.CeilToInt(gridSize.x / cellSize);
        int gridHeight = Mathf.CeilToInt(gridSize.y / cellSize);
        walkableGrid = new bool[gridWidth, gridHeight];

        Vector3 startPos = new Vector3(-gridSize.x / 2, 0, -gridSize.y / 2);

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector3 worldPos = startPos + new Vector3(x * cellSize, 0, z * cellSize);
                walkableGrid[x, z] = NavMesh.SamplePosition(worldPos, out NavMeshHit hit, cellSize / 2, NavMesh.AllAreas);
            }
        }

        Debug.Log($"NavMesh Grid Generated: {gridWidth}x{gridHeight}");
    }

    // ѕроверка, можно ли ходить в точке (в мировых координатах)
    public bool IsWalkable(Vector3 worldPosition)
    {
        Vector3 localPos = worldPosition + new Vector3(gridSize.x / 2, 0, gridSize.y / 2);
        int x = Mathf.FloorToInt(localPos.x / cellSize);
        int z = Mathf.FloorToInt(localPos.z / cellSize);
        
        if (x >= 0 && x < walkableGrid.GetLength(0) && z >= 0 && z < walkableGrid.GetLength(1))
        {
            return walkableGrid[x, z];
        }
        return false;
    }

    public void SetWaypoints()
    {
        Item[] items = gameObject.GetComponentsInChildren<Item>();

        /*
        for (int i = 0; i < items.Length; i++)
        {
            switch (items[i].type)
            {
                case Item.ItemType.Weapon:
                    weapons.Add(new Tuple<Vector3, Item.WeaponType>(items[i].transform.position, items[i].weaponType));
                    break;
                case Item.ItemType.AmmoPack:
                    ammoPacks.Add(new Tuple<Vector3, Item.AmmoType>(items[i].transform.position, items[i].ammoType));
                    break;
                case Item.ItemType.HealthPack:
                    healthPacks.Add(items[i].transform.position);
                    break;
                case Item.ItemType.ArmorPack:
                    armorPacks.Add(items[i].transform.position);
                    break;
            }
        }
        */

        for (int i = 0; i < items.Length; i++)
        {
            switch (items[i].type)
            {
                case Item.ItemType.Weapon:
                    weapons.Add(items[i]);
                    break;
                case Item.ItemType.AmmoPack:
                    ammoPacks.Add(items[i]);
                    break;
                case Item.ItemType.HealthPack:
                    healthPacks.Add(items[i]);
                    break;
                case Item.ItemType.ArmorPack:
                    armorPacks.Add(items[i]);
                    break;
            }
        }
    }

    private void Awake()
    {
        playerUI = gameObject.GetComponentInChildren<PlayerUI>();

        GenerateNavMeshGrid();
        SetWaypoints();
    }

    private void Update()
    {
        if (roundTime > 0f) { roundTime -= Time.deltaTime; }
        if (roundTime <= 0f)
        {
            // StopMatch();
        }

        if (playerUI != null) 
            playerUI.SetTimerCountdown((int) roundTime);
    }
}
