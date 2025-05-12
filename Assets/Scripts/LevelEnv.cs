using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class LevelEnv : MonoBehaviour
{
    // 0 - оружие, 1 - патроны, 2 - аптечки, 3 - брон€
    public Vector3[][] waypoints = new Vector3[4][];

    public float cellSize = 0.5f;
    public Vector2 gridSize = new Vector2(100f, 100f);
    public bool[,] walkableGrid;

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

                // TODO добавить динамическое добавление предметов в waypoints
                // „ерез Raycast hit if (IItem) {switch (IItem.type)}
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

    private void Awake()
    {
        GenerateNavMeshGrid();
    }
}
