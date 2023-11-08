using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFieldCellLink
{
    public GameFieldCell cell;
    public DoorController door = null;
    public bool isLocked
    {
        get { return (door != null && door.isLocked); }
    }
    public GameFieldCellLink(GameFieldCell cell, DoorController door = null)
    {
        this.cell = cell;
        this.door = door;
    }
}
public class GameFieldCell
{
    public Vector3 coords;
    public Vector2Int intCoords;

    public GameFieldCellLink leftCell = null;
    public GameFieldCellLink rightCell = null;
    public GameFieldCellLink topCell = null;
    public GameFieldCellLink bottomCell = null;

    public bool isWalkable;

    public int roomIndex = 0;
    public bool isMaseCell = false;
}
public class GameField : MonoBehaviour
{
    public List<EnemyController> enemyPrefabs;
    public int enemyCount = 5;

    public GameFieldCell[,] cells;

    public LayerMask walkableMask;
    public LayerMask unwalkableMask;
    public LayerMask doorMask;

    public float cellSize = 1;

    public int width = 20;
    public int height = 15;

    public int playerSpawnX = 1, playerSpawnY = 1;


    public void InitSettings(LevelSettings settings, int level)
    {
        enemyCount = settings.enemyCount;
        //enemyPrefabs = settings.enemyPrefabs;
        enemyPrefabs.Clear();
        foreach (EnemyController prefab in settings.enemyPrefabs) if (prefab.CheckLevel(level)) enemyPrefabs.Add(prefab);
        cellSize = settings.cellSize;
        width = settings.width;
        height = settings.height;
        playerSpawnX = settings.playerSpawn.x;
        playerSpawnY = settings.playerSpawn.y;
    }
    public void Init()
    {        
        cells = generateField();
    }
    GameFieldCell[,] generateField()
    {
        List<Vector2Int> filledPoints = new List<Vector2Int>();

        for (int y = -1; y < 2; ++y)
        {
            for (int x = -1; x < 2; ++x)
            {
                filledPoints.Add(new Vector2Int(playerSpawnX + x, playerSpawnY + y));
            }
        }

        GameFieldCell[,] field = new GameFieldCell[width, height];

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                field[x, y] = new GameFieldCell();
                field[x, y].intCoords.x = x;
                field[x, y].intCoords.y = y;
                field[x, y].coords = new Vector3((x + .5f - width / 2f) * cellSize, 0, (y + .5f - height / 2f) * cellSize) + transform.position;

                field[x, y].isWalkable = (!Physics.Raycast(new Vector3((x + .5f - width / 2f) * cellSize + transform.position.x, 100, (y + .5f - height / 2f) * cellSize + transform.position.z), Vector3.down, 200, unwalkableMask));
                RaycastHit hit;
                if (field[x, y].isWalkable && Physics.Raycast(new Vector3((x + .5f - width / 2f) * cellSize + transform.position.x, 100, (y + .5f - height / 2f) * cellSize + transform.position.z), Vector3.down, out hit, 200, walkableMask))
                {
                    field[x, y].isWalkable = true;
                    MazeCellPrefab mc = hit.collider.GetComponent<MazeCellPrefab>();
                    if(mc == null)mc = hit.collider.GetComponentInParent<MazeCellPrefab>();
                    if (mc != null) field[x, y].roomIndex = mc.roomIndex;
                    field[x, y].isMaseCell = (mc != null);
                }
                else field[x, y].isWalkable = false;

            }
        }
        List<Vector2Int> walkablePoints = new List<Vector2Int>();
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                if (field[x, y] == null ) continue;

                if (x > 0 && field[x - 1, y] != null && !hasWall(field[x, y], field[x - 1, y]) && field[x, y].leftCell == null) field[x, y].leftCell = new GameFieldCellLink(field[x - 1, y], getDoor(field[x, y], field[x - 1, y]));
                if (x < width - 1 && field[x + 1, y] != null && !hasWall(field[x, y], field[x + 1, y]) && field[x, y].rightCell == null) field[x, y].rightCell = new GameFieldCellLink(field[x + 1, y], getDoor(field[x, y], field[x + 1, y]));
                if (y > 0 && field[x, y - 1] != null && !hasWall(field[x, y], field[x, y - 1]) && field[x, y].bottomCell == null) field[x, y].bottomCell = new GameFieldCellLink(field[x, y - 1], getDoor(field[x, y], field[x, y - 1]));
                if (y < height - 1 && field[x, y + 1] != null && !hasWall(field[x, y], field[x, y + 1]) && field[x, y].topCell == null) field[x, y].topCell = new GameFieldCellLink(field[x, y + 1], getDoor(field[x, y], field[x, y + 1]));

                if (field[x, y].roomIndex<0 || field[x, y].isMaseCell) continue;
                if (field[x, y].isWalkable && !filledPoints.Contains(field[x, y].intCoords)) walkablePoints.Add(field[x, y].intCoords);
            }
        }

        if (enemyPrefabs.Count > 0)
        {
            for (int i = 0; i < enemyCount; ++i)
            {
                EnemyController enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
                if (walkablePoints.Count == 0) break;
                Vector2Int v = walkablePoints[Random.Range(0, walkablePoints.Count)];
                walkablePoints.Remove(v);
                RaycastHit hit;
                Vector3 pos = new Vector3((v.x + .5f - width / 2f) * cellSize + transform.position.x, transform.position.y, (v.y + .5f - height / 2f) * cellSize + transform.position.z);
                if (Physics.Raycast(pos + Vector3.up * 100, Vector3.down, out hit, 200, walkableMask)) pos = hit.point;
                EnemyController enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
                if (enemy != null)
                {
                    enemy.coords = v;
                }
                filledPoints.Add(v);
            }
        }
        return field;
    }
    private bool hasWall(GameFieldCell a, GameFieldCell b)
    {
        Vector3 v = (a.coords + b.coords) / 2;
        return (Physics.Raycast(new Vector3(v.x, 100, v.z), Vector3.down, 200, unwalkableMask));
    }

    private DoorController getDoor(GameFieldCell a, GameFieldCell b)
    {
        Vector3 v = (a.coords + b.coords) / 2;
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(v.x, 100, v.z), Vector3.down, out hit, 200, doorMask))
        {
            //Debug.Log(hit.collider.name);
            return hit.collider.GetComponent<DoorController>();
        }
        else return null;
    }


    private void OnDrawGizmos()
    {
        if (cells != null)
        {
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    if (cells[x, y] == null) continue;
                    if (cells[x, y].isWalkable) Gizmos.color = new Color(0, 1, 0, 0.5f);
                    else Gizmos.color = new Color(1, 0, 0, 0.5f);
                    //Gizmos.DrawCube(new Vector3(cells[x, y, l].coords.x, cells[x, y, l].coords.y + layerHeight * .5f, cells[x, y, l].coords.z), new Vector3(cellSize, layerHeight, cellSize) * .9f);

                    if (cells[x, y].isWalkable)
                    {
                        bool f;
                        if (cells[x, y].leftCell != null && cells[x, y].leftCell.cell.isWalkable)
                        {
                            Gizmos.color = new Color(0, 0, 1, 0.5f);
                            f = true;
                        }
                        else
                        {
                            Gizmos.color = new Color(1, 0, 0, 0.5f);
                            f = false;
                        }
                        if (!f) Gizmos.DrawCube(new Vector3(cells[x, y].coords.x - .45f * cellSize, 1, cells[x, y].coords.z), new Vector3(cellSize * .1f, 2, cellSize));

                        if (cells[x, y].rightCell != null && cells[x, y].rightCell.cell.isWalkable)
                        {
                            Gizmos.color = new Color(0, 0, 1, 0.5f);
                            f = true;
                        }
                        else
                        {
                            Gizmos.color = new Color(1, 0, 0, 0.5f);
                            f = false;
                        }
                        if (!f) Gizmos.DrawCube(new Vector3(cells[x, y].coords.x + .45f * cellSize, 1, cells[x, y].coords.z), new Vector3(cellSize * .1f, 2, cellSize));

                        if (cells[x, y].bottomCell != null && cells[x, y].bottomCell.cell.isWalkable)
                        {
                            Gizmos.color = new Color(0, 0, 1, 0.5f);
                            f = true;
                        }
                        else
                        {
                            Gizmos.color = new Color(1, 0, 0, 0.5f);
                            f = false;
                        }
                        if (!f) Gizmos.DrawCube(new Vector3(cells[x, y].coords.x, 1, cells[x, y].coords.z - .45f * cellSize), new Vector3(cellSize, 2, cellSize * .1f));

                        if (cells[x, y].topCell != null && cells[x, y].topCell.cell.isWalkable)
                        {
                            Gizmos.color = new Color(0, 0, 1, 0.5f);
                            f = true;
                        }
                        else
                        {
                            Gizmos.color = new Color(1, 0, 0, 0.5f);
                            f = false;
                        }
                        if (!f) Gizmos.DrawCube(new Vector3(cells[x, y].coords.x, 1, cells[x, y].coords.z + .45f * cellSize), new Vector3(cellSize, 2, cellSize * .1f));
                    }
                }
            }

        }
    }

}
