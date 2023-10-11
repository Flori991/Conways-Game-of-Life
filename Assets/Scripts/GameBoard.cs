using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameBoard : MonoBehaviour
{
    [SerializeField] private Tilemap currentState;
    [SerializeField] private Tilemap nextState;
    [SerializeField] private Tile aliveTile;
    [SerializeField] private Tile deadTile;
    [SerializeField] private Pattern pattern;
    [SerializeField] private float updateInterval = 0.05f;

    private HashSet<Vector3Int> aliveCells;
    private HashSet<Vector3Int> cellsToCheck;

    public int population { get; private set; }
    public int iterations { get; private set; }
    public float time { get; private set; }

    private void Awake()
    {
        aliveCells = new HashSet<Vector3Int>();
        cellsToCheck = new HashSet<Vector3Int>();
    }

    private void Start()
    {
        SetInitialPattern(pattern);
    }

    private void SetInitialPattern(Pattern pattern)
    {
        Clear();

        Vector2Int center = pattern.GetCenter();

        for (int i = 0; i < pattern.cells.Length; i++)
        {
            Vector3Int cell = (Vector3Int)(pattern.cells[i] - center);
            currentState.SetTile(cell, aliveTile);
            aliveCells.Add(cell);
        }

        population = aliveCells.Count;
    }

    private void Clear()
    {
        currentState.ClearAllTiles();
        nextState.ClearAllTiles();
        aliveCells.Clear();
        cellsToCheck.Clear();

        population = 0;
        iterations = 0;
        time = 0f;
    }

    private void OnEnable()
    {
        StartCoroutine(Simulate());
    }

    private IEnumerator Simulate()
    {
        // Cache the interval so it is not created every time, saving memory
        // But cannot change interval during loop
        //var interval = new WaitForSeconds(updateInterval);

        while (enabled)
        {
            UpdateState();

            population = aliveCells.Count;
            iterations++;
            time += updateInterval;

            yield return new WaitForSeconds(updateInterval);
        }
    }

    private void UpdateState()
    {
        foreach (var aliveCell in aliveCells)
        {
            GatherCellsToCheck(aliveCell);
        }
        foreach (Vector3Int cellToCheck in cellsToCheck)
        {
            ApplyGameRules(cellToCheck);
        }

        Tilemap bufferOldState = currentState;
        currentState = nextState;
        nextState = bufferOldState;
    }

    private void GatherCellsToCheck(Vector3Int cell)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                cellsToCheck.Add(cell + new Vector3Int(x, y, 0));
            }
        }

    }

    private void ApplyGameRules(Vector3Int cell)
    {
        int neighbors = CountNeighbors(cell);
        bool alive = IsAlive(cell);

        if (!alive && neighbors == 3)
        {
            nextState.SetTile(cell, aliveTile);
            aliveCells.Add(cell);
        }
        else if (alive && (neighbors > 3 || neighbors < 2))
        {
            nextState.SetTile(cell, deadTile);
            aliveCells.Remove(cell);
        }
        else
        {
            nextState.SetTile(cell, currentState.GetTile(cell));
        }
    }

    private int CountNeighbors(Vector3Int cell)
    {
        int count = 0;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int neighbor = cell + new Vector3Int(x, y, 0);

                if (x == 0 && y == 0)
                {
                    continue;
                }
                else if (IsAlive(neighbor))
                {
                    count++;
                }
            }
        }

        return count;
    }

    private bool IsAlive(Vector3Int cell)
    {
        return currentState.GetTile(cell) == aliveTile;
    }
}