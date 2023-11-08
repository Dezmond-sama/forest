using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeCell
{
    public int x, y;
    public bool hasBottomWall = true, hasLeftWall = true, hasRightWall = true, hasTopWall = true;
    public bool visited = false;
    public bool room = false;
    public int roomNumber = 0;
    public bool locked = false;
    //public int exitCount
    //{
    //    get
    //    {
    //        return (hasTopWall ? 0 : 1) +
    //               (hasBottomWall ? 0 : 1) +
    //               (hasLeftWall ? 0 : 1) +
    //               (hasRightWall ? 0 : 1);
    //    }
    //}
}

public class MazeDoor
{
    public int doorIndex = 0;
    public MazeCell cell1, cell2;
}

public class Maze : MonoBehaviour
{
    public int width;
    public int height;
    public float cellSize = 1;
    public bool createOuterWalls = true;
    public bool singleWalls = false;

    public Vector2Int minimumRoomSize = new Vector2Int(2, 2);
    public Vector2Int maximumRoomSize = new Vector2Int(5, 5);


    public int roomCount = 0;
    public int lockedCount = 0;
    public int coinsInLockedCount = 0;
    public int coinsInOpenCount = 0;
    public int tryingPlaceRoomIterations = 5;
    public int cyclesCount = 0;
    public bool removeDeadEnds = false;

    //public GameObject cellPrefab;
    public List<DoorController> doorPrefabs;
    public ShadowBlock shadowBlock;
    public List<KeyController> keyPrefabs;
    public List<Collectable> coinPrefabs;
    public List<PortalController> portalPrefabs;

    public List<MazeElement> cellPrefabs;
    public List<MazeCellPrefab> cellPrefabsWithWalls;
    public List<MazeCellPrefab> roomElemPrefabs;
    public List<EnemyController> enemyPrefabs;
    public int enemyCount = 5;


    public List<LevelElement> roomConnectorPrefabs;

    public List<LevelElement> wallCornerPrefabs;

    private MazeCell[,] cells;
    private List<MazeDoor> doors;

    public Vector2Int startPoint;

    public Vector2Int exitPoint;

    public PortalController portal = null;

    private List<Vector2Int> filledPoints = new List<Vector2Int>();

    private int GetExitCount(MazeCell cell)
    {
        int count = 4;
        if (cell.hasLeftWall || (cell.x > 0 && cells[cell.x - 1, cell.y].hasRightWall)) count--;
        if (cell.hasRightWall || (cell.x < width - 1 && cells[cell.x + 1, cell.y].hasLeftWall)) count--;
        if (cell.hasBottomWall || (cell.y > 0 && cells[cell.x, cell.y - 1].hasTopWall)) count--;
        if (cell.hasTopWall || (cell.y < height - 1 && cells[cell.x, cell.y + 1].hasBottomWall)) count--;

        return count;
    }

    public void initSettings(LevelSettings settings,int level)
    {
        Debug.Log("initSettings");
        width = settings.labyrinthWidth;
        height = settings.labyrinthHeight;
        cellSize = settings.labyrinthCellSize;

        createOuterWalls = settings.createOuterWalls;
        singleWalls = settings.singleWalls;
        minimumRoomSize = settings.minimumRoomSize;
        maximumRoomSize = settings.maximumRoomSize;

        roomCount = settings.roomCount;
        lockedCount = settings.lockedCount;
        coinsInLockedCount = settings.coinsInLockedCount;
        coinsInOpenCount = settings.coinsInOpenCount;
        tryingPlaceRoomIterations = settings.tryingPlaceRoomIterations;
        cyclesCount = settings.cyclesCount;
        removeDeadEnds = settings.removeDeadEnds;

        doorPrefabs = settings.doorPrefabs;

        shadowBlock = settings.shadowBlock;
        keyPrefabs = settings.keyPrefabs;
        coinPrefabs = settings.coinPrefabs;

        cellPrefabs.Clear();
        foreach (MazeElement prefab in settings.cellPrefabs) if(prefab.CheckLevel(level)) cellPrefabs.Add(prefab);
        cellPrefabsWithWalls.Clear();
        foreach (MazeCellPrefab prefab in settings.cellPrefabsWithWalls) if (prefab.CheckLevel(level)) cellPrefabsWithWalls.Add(prefab);
        roomElemPrefabs.Clear();
        foreach (MazeCellPrefab prefab in settings.roomElemPrefabs) if (prefab.CheckLevel(level)) roomElemPrefabs.Add(prefab);
        roomConnectorPrefabs.Clear();
        foreach (LevelElement prefab in settings.roomConnectorPrefabs) if (prefab.CheckLevel(level)) roomConnectorPrefabs.Add(prefab);
        wallCornerPrefabs.Clear();
        foreach (LevelElement prefab in settings.wallCornerPrefabs) if (prefab.CheckLevel(level)) wallCornerPrefabs.Add(prefab);
        enemyPrefabs.Clear();
        foreach (EnemyController prefab in settings.enemyPrefabs) if (prefab.CheckLevel(level)) enemyPrefabs.Add(prefab);
        //cellPrefabs = settings.cellPrefabs;
        //cellPrefabsWithWalls = settings.cellPrefabsWithWalls;
        //roomElemPrefabs = settings.roomElemPrefabs;
        //roomConnectorPrefabs = settings.roomConnectorPrefabs;
        //wallCornerPrefabs = settings.wallCornerPrefabs;
        //enemyPrefabs = settings.enemyPrefabs;
        enemyCount = settings.labyrinthEnemyCount;

        startPoint = settings.startPoint;
        exitPoint = settings.exitPoint;
    }

    public void initMaze()
    {
        if (cellPrefabsWithWalls.Count == 0) return;

        for (int y = -1; y < 2; ++y)
        {
            for (int x = -1; x < 2; ++x)
            {
                filledPoints.Add(new Vector2Int(startPoint.x + x, startPoint.y + y));
            }
        }

        doors = new List<MazeDoor>();
        //cells = new MazeCell[width + 1, height + 1];
        cells = new MazeCell[width, height];
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                cells[x, y] = new MazeCell();
                cells[x, y].x = x;
                cells[x, y].y = y;
                if (x == startPoint.x && y == startPoint.y) cells[x, y].visited = true;
            }
        }

        int iter = 1000;

        while (iter > 0)
        {
            if (AddRoom(new Vector2Int(2,1), -1, true,true,0))
            {
                Debug.Log("AddRoom");
                break;
            }
            else
            {
                iter--;
            }
        }

        iter = tryingPlaceRoomIterations;
        int number = 1;
        while (roomCount > 0 && iter > 0)
        {
            if (AddRoom(new Vector2Int(
                UnityEngine.Random.Range(minimumRoomSize.x, maximumRoomSize.x + 1),
                UnityEngine.Random.Range(minimumRoomSize.y, maximumRoomSize.y + 1)), number, lockedCount > 0))
            {
                lockedCount--;
                roomCount--;
                number++;
                iter = tryingPlaceRoomIterations;
            }
            else
            {
                iter--;
            }
        }

        number--;

        //makeExit(startPoint);
        //makeExit(exitPoint);
        for (int x = 0; x < width; ++x)
        {
            cells[x, height - 1].hasRightWall = createOuterWalls;
            cells[x, 0].hasLeftWall = createOuterWalls;
        }

        for (int y = 0; y < height; ++y)
        {
            cells[width - 1, y].hasBottomWall = createOuterWalls;
            cells[0, y].hasTopWall = createOuterWalls;
        }

        RemoveWallsWithBacktracker();
        MakeCycles(cyclesCount);
        if(removeDeadEnds) RemoveDeadEnds();

        if (singleWalls)
        {
            for (int x = 0; x < width - 1; ++x)
            {
                for (int y = 0; y < height - 1; ++y)
                {
                    if (GetExitCount(cells[x, y + 1]) > 0) cells[x, y].hasTopWall = false;
                    if (GetExitCount(cells[x + 1, y]) > 0) cells[x, y].hasRightWall = false;
                }
            }
        }
        InstantiateMaze();
    }

    public bool AddRoom(Vector2Int roomSize, int roomNumber, bool locked, bool isExitRoom = false, int perimeter = 1)
    {
        List<Vector2Int> roomStartCoords = new List<Vector2Int>();

        for (int x = perimeter; x < width - perimeter - roomSize.x; ++x)
        {
            for (int y = perimeter; y < height - perimeter - roomSize.y; ++y)
            {
                Vector2Int roomCoords = new Vector2Int(x, y);
                if (CanPlaceRoom(roomCoords, roomSize,perimeter))
                {
                    roomStartCoords.Add(roomCoords);
                }
            }
        }
        //Debug.Log(roomStartCoords.Count);
        if (roomStartCoords.Count > 0)
        {
            Vector2Int roomCoords = roomStartCoords[UnityEngine.Random.Range(0, roomStartCoords.Count)];
            PlaceRoom(roomCoords, roomSize, roomNumber, locked, isExitRoom);
            return true;
        }
        return false;
    }

    bool CanPlaceRoom(Vector2Int roomCoords, Vector2Int roomSize, int perimeter)
    {
        for (int x = roomCoords.x - perimeter; x < roomCoords.x + roomSize.x + perimeter; ++x)
        {
            for (int y = roomCoords.y - perimeter; y < roomCoords.y + roomSize.y + perimeter; ++y)
            {
                if (x >= width || y >= height || x < 0 || y < 0 || cells[x, y] == null || cells[x, y].room || cells[x, y].visited) return false;
            }
        }
        return true;
    }

    void PlaceRoom(Vector2Int roomCoords, Vector2Int roomSize, int roomNumber, bool locked, bool isExitRoom)
    {
        List<MazeCell> exitList = new List<MazeCell>();
        for (int x = roomCoords.x; x < roomCoords.x + roomSize.x; ++x)
        {
            for (int y = roomCoords.y; y < roomCoords.y + roomSize.y; ++y)
            {
                cells[x, y].room = true;
                cells[x, y].visited = true;

                cells[x, y].hasBottomWall = y == roomCoords.y;
                cells[x, y].hasTopWall = y == roomCoords.y + roomSize.y - 1;
                cells[x, y].hasLeftWall = x == roomCoords.x;
                cells[x, y].hasRightWall = x == roomCoords.x + roomSize.x - 1;
                cells[x, y].roomNumber = roomNumber;
                cells[x, y].locked = locked || isExitRoom;
                if (x == roomCoords.x || y == roomCoords.y || x == roomCoords.x + roomSize.x - 1 || y == roomCoords.y + roomSize.y - 1)
                {
                    exitList.Add(cells[x, y]);
                }
            }
        }
        if (exitList.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, exitList.Count);
        
            List<MazeCell> neighbours = new List<MazeCell>();
            
            int x = exitList[index].x;
            int y = exitList[index].y;
            if (x > 0 && !cells[x - 1, y].room) neighbours.Add(cells[x - 1, y]);
            if (y > 0 && !cells[x, y - 1].room) neighbours.Add(cells[x, y - 1]);
            if (x < width - 1 && !cells[x + 1, y].room) neighbours.Add(cells[x + 1, y]);
            if (y < height - 1 && !cells[x, y + 1].room) neighbours.Add(cells[x, y + 1]);
            
            if (neighbours.Count > 0)
            {
                MazeCell cell = neighbours[UnityEngine.Random.Range(0, neighbours.Count)];
                RemoveWall(exitList[index], cell);
            }
            exitList.Remove(exitList[index]);
        }
        if (isExitRoom && exitList.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, exitList.Count);

            List<MazeCell> neighbours = new List<MazeCell>();

            exitPoint.x = exitList[index].x;
            exitPoint.y = exitList[index].y;

            int x = exitPoint.x;
            int y = exitPoint.y;

            if (x > 0 && !cells[x - 1, y].room) neighbours.Add(cells[x - 1, y]);
            if (y > 0 && !cells[x, y - 1].room) neighbours.Add(cells[x, y - 1]);
            if (x < width - 1 && !cells[x + 1, y].room) neighbours.Add(cells[x + 1, y]);
            if (y < height - 1 && !cells[x, y + 1].room) neighbours.Add(cells[x, y + 1]);

            if (neighbours.Count > 0)
            {
                MazeCell cell = neighbours[UnityEngine.Random.Range(0, neighbours.Count)];
                Vector2Int dir = new Vector2Int(cell.x, cell.y);
                InstantiatePortal(exitPoint,dir);
            }

            
        }
    }

    public void InstantiateMaze()
    {
        if (cellPrefabs.Count > 0)
        {
            List<MazeElement> elems4 = new List<MazeElement>();
            List<MazeElement> elems3 = new List<MazeElement>();
            List<MazeElement> elems2I = new List<MazeElement>();
            List<MazeElement> elems2L = new List<MazeElement>();
            List<MazeElement> elems1 = new List<MazeElement>();

            foreach (MazeElement elem in cellPrefabs)
            {
                switch (elem.exitCount)
                {
                    case 1:
                        elems1.Add(elem);
                        break;
                    case 2:
                        if (elem.hasBottomExit == elem.hasTopExit) elems2I.Add(elem);
                        else elems2L.Add(elem);
                        break;
                    case 3:
                        elems3.Add(elem);
                        break;
                    case 4:
                        elems4.Add(elem);
                        break;
                }
            }
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    MazeElement elem = null;
                    switch (GetExitCount(cells[x, y]))
                    {
                        case 1:
                            if (elems1.Count > 0) elem = elems1[UnityEngine.Random.Range(0, elems1.Count)];
                            break;
                        case 2:
                            if (cells[x, y].hasTopWall == cells[x, y].hasBottomWall)
                            {
                                if (elems2I.Count > 0) elem = elems2I[UnityEngine.Random.Range(0, elems2I.Count)];
                            }
                            else
                            {
                                if (elems2L.Count > 0) elem = elems2L[UnityEngine.Random.Range(0, elems2L.Count)];
                            }
                            break;
                        case 3:
                            if (elems3.Count > 0) elem = elems3[UnityEngine.Random.Range(0, elems3.Count)];
                            break;
                        case 4:
                            if (elems4.Count > 0) elem = elems4[UnityEngine.Random.Range(0, elems4.Count)];
                            break;
                    }
                    if (elem != null)
                    {
                        MazeElement cell = Instantiate(elem, new Vector3((x + .5f - width / 2f) * cellSize + transform.position.x, transform.position.y, (y + .5f - height / 2f) * cellSize + transform.position.z), Quaternion.Euler(0, elem.GetRotation(!cells[x, y].hasTopWall, !cells[x, y].hasBottomWall, !cells[x, y].hasLeftWall, !cells[x, y].hasRightWall), 0));
                        if (roomConnectorPrefabs.Count > 0)
                        {
                            if (!cells[x, y].hasBottomWall) {
                                Instantiate(roomConnectorPrefabs[UnityEngine.Random.Range(0, roomConnectorPrefabs.Count)], new Vector3((x + .5f - width / 2f) * cellSize + transform.position.x, transform.position.y, (y - height / 2f) * cellSize + transform.position.z), Quaternion.Euler(0f, 0f, 0f));
                            }
                            if (y == height - 1 && !cells[x, y].hasTopWall)
                            {
                                Instantiate(roomConnectorPrefabs[UnityEngine.Random.Range(0, roomConnectorPrefabs.Count)], new Vector3((x + .5f - width / 2f) * cellSize + transform.position.x, transform.position.y, (y + 1f - height / 2f) * cellSize + transform.position.z), Quaternion.Euler(0f, 0f, 0f));
                            }
                            if (!cells[x, y].hasLeftWall)
                            {
                                Instantiate(roomConnectorPrefabs[UnityEngine.Random.Range(0, roomConnectorPrefabs.Count)], new Vector3((x - width / 2f) * cellSize + transform.position.x, transform.position.y, (y + .5f - height / 2f) * cellSize + transform.position.z), Quaternion.Euler(0f, 90f, 0f));
                            }
                            if (x == width - 1 && !cells[x, y].hasRightWall)
                            {
                                Instantiate(roomConnectorPrefabs[UnityEngine.Random.Range(0, roomConnectorPrefabs.Count)], new Vector3((x + 1 - width / 2f) * cellSize + transform.position.x, transform.position.y, (y + .5f - height / 2f) * cellSize + transform.position.z), Quaternion.Euler(0f, 90f, 0f));
                            }
                        }
                    }
                }
            }
        }
        else
        {
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    if (GetExitCount(cells[x, y]) == 0) continue;

                    if (wallCornerPrefabs.Count > 0)
                    {
                        if (cells[x, y].hasLeftWall ||
                           cells[x, y].hasBottomWall ||
                           (x > 0 && (cells[x - 1, y].hasRightWall || cells[x - 1, y].hasBottomWall)) ||
                           (y > 0 && (cells[x, y - 1].hasTopWall || cells[x, y - 1].hasLeftWall)))
                        {
                            Instantiate(wallCornerPrefabs[UnityEngine.Random.Range(0, wallCornerPrefabs.Count)], new Vector3((x - width / 2f) * cellSize + transform.position.x, transform.position.y, (y - height / 2f) * cellSize + transform.position.z), Quaternion.identity);
                        }

                        if ((y == height - 1 || GetExitCount(cells[x, y + 1]) == 0) &&
                           (cells[x, y].hasLeftWall ||
                            cells[x, y].hasTopWall))
                        {
                            Instantiate(wallCornerPrefabs[UnityEngine.Random.Range(0, wallCornerPrefabs.Count)], new Vector3((x - width / 2f) * cellSize + transform.position.x, transform.position.y, (y + 1 - height / 2f) * cellSize + transform.position.z), Quaternion.identity);
                        }

                        if ((x == width - 1 || GetExitCount(cells[x + 1, y]) == 0) &&
                           (cells[x, y].hasRightWall ||
                            cells[x, y].hasBottomWall))
                        {
                            Instantiate(wallCornerPrefabs[UnityEngine.Random.Range(0, wallCornerPrefabs.Count)], new Vector3((x + 1 - width / 2f) * cellSize + transform.position.x, transform.position.y, (y - height / 2f) * cellSize + transform.position.z), Quaternion.identity);
                        }

                        if ((y == height - 1 || GetExitCount(cells[x, y + 1]) == 0) &&
                            (x == width - 1 || GetExitCount(cells[x + 1, y]) == 0) &&
                            (cells[x, y].hasRightWall ||
                           cells[x, y].hasTopWall))
                        {
                            Instantiate(wallCornerPrefabs[UnityEngine.Random.Range(0, wallCornerPrefabs.Count)], new Vector3((x + 1 - width / 2f) * cellSize + transform.position.x, transform.position.y, (y + 1 - height / 2f) * cellSize + transform.position.z), Quaternion.identity);
                        }
                    }


                    MazeCellPrefab cell = cells[x, y].room ? 
                        Instantiate(roomElemPrefabs[UnityEngine.Random.Range(0, roomElemPrefabs.Count)], new Vector3((x + .5f - width / 2f) * cellSize + transform.position.x, transform.position.y, (y + .5f - height / 2f) * cellSize + transform.position.z), Quaternion.identity) :
                        Instantiate(cellPrefabsWithWalls[UnityEngine.Random.Range(0, cellPrefabsWithWalls.Count)], new Vector3((x + .5f - width / 2f) * cellSize + transform.position.x, transform.position.y, (y + .5f - height / 2f) * cellSize + transform.position.z), Quaternion.identity);
                    if (cell != null)
                    {
                        if (cell.leftWall != null) cell.leftWall.SetActive(cells[x, y].hasLeftWall);
                        if (cell.bottomWall != null) cell.bottomWall.SetActive(cells[x, y].hasBottomWall);
                        if (cell.rightWall != null) cell.rightWall.SetActive(cells[x, y].hasRightWall);
                        if (cell.topWall != null) cell.topWall.SetActive(cells[x, y].hasTopWall);
                        cell.roomIndex = cells[x, y].roomNumber;
                    }
                    if(cells[x, y].locked)
                    {
                        ShadowBlock sh = Instantiate(shadowBlock, new Vector3((x + .5f - width / 2f) * cellSize + transform.position.x, transform.position.y, (y + .5f - height / 2f) * cellSize + transform.position.z), Quaternion.identity);
                        sh.index = cells[x, y].roomNumber;
                    }
                }
            }
        }
        foreach (MazeDoor door in doors)
        {
            InstantiateDoor(door);
            if(door.doorIndex > 0) PlaceKey(door.doorIndex,false);
        }
        InstantiateEnemies();
        InstantiateTreasures();
    }

    private void InstantiateEnemies()
    {
        if (enemyPrefabs.Count == 0) return;
        List<Vector2Int> points = new List<Vector2Int>();


        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                if (cells[x, y] == null || GetExitCount(cells[x, y]) == 0 || cells[x, y].roomNumber == -1) continue;
                Vector2Int temp = new Vector2Int(x, y);
                if (filledPoints.Contains(temp)) continue;
                points.Add(temp);
            }
        }

        for (int i = 0; i < enemyCount; ++i)
            {
                EnemyController enemyPrefab = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Count)];
                if (points.Count == 0) break;
                Vector2Int v = points[UnityEngine.Random.Range(0, points.Count)];
                points.Remove(v);
                Vector3 pos = new Vector3((v.x + .5f - width / 2f) * cellSize + transform.position.x, transform.position.y, (v.y + .5f - height / 2f) * cellSize + transform.position.z);
                EnemyController enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
                if (enemy != null)
                {
                    enemy.coords = v;
                }
                filledPoints.Add(v);
            }
        
    }
    private void InstantiateTreasures()
    {
        if (coinPrefabs.Count == 0) return;
        
        List<Vector2Int> openPoints = new List<Vector2Int>();
        List<Vector2Int> lockedPoints = new List<Vector2Int>();

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                if (cells[x, y] == null|| GetExitCount(cells[x, y]) == 0 || cells[x, y].roomNumber == -1) continue;
                Vector2Int temp = new Vector2Int(x, y);
                if (filledPoints.Contains(temp))continue;
                if (cells[x, y].locked) lockedPoints.Add(temp);
                else openPoints.Add(temp);
            }
        }

        while(coinsInOpenCount > 0 && openPoints.Count > 0)
        {
            Collectable coinPrefab = coinPrefabs[UnityEngine.Random.Range(0, coinPrefabs.Count)];

            int index = UnityEngine.Random.Range(0, openPoints.Count);
            Vector3 point = new Vector3((openPoints[index].x + .5f - width / 2f) * cellSize + transform.position.x, transform.position.y, (openPoints[index].y + .5f - height / 2f) * cellSize + transform.position.z);
            Collectable coin = Instantiate(coinPrefab, point, Quaternion.identity);
            coin.coords = openPoints[index];
            openPoints.Remove(openPoints[index]);
            coinsInOpenCount--;
        }

        while (coinsInLockedCount > 0 && lockedPoints.Count > 0)
        {
            Collectable coinPrefab = coinPrefabs[UnityEngine.Random.Range(0, coinPrefabs.Count)];

            int index = UnityEngine.Random.Range(0, lockedPoints.Count);
            Vector3 point = new Vector3((lockedPoints[index].x + .5f - width / 2f) * cellSize + transform.position.x, transform.position.y, (lockedPoints[index].y + .5f - height / 2f) * cellSize + transform.position.z);
            Collectable coin = Instantiate(coinPrefab, point, Quaternion.identity);
            coin.coords = lockedPoints[index];
            lockedPoints.Remove(lockedPoints[index]);
            coinsInLockedCount--;

        }
    }

    private void InstantiateDoor(MazeDoor door)
    {
        if (door == null || door.cell1 == null || door.cell2 == null || doorPrefabs.Count == 0) return;
        DoorController doorPrefab = doorPrefabs[UnityEngine.Random.Range(0, doorPrefabs.Count)];
        Vector3 point = new Vector3((door.cell1.x + door.cell2.x + 1f - width) / 2f * cellSize + transform.position.x, transform.position.y, (door.cell1.y + door.cell2.y + 1f - height) / 2f * cellSize + transform.position.z);
        float a = 0;
        if (door.cell1.x > door.cell2.x) a = 90f;
        else if (door.cell1.y > door.cell2.y) a = 180f;
        else if (door.cell1.x < door.cell2.x) a = 270f;
        Quaternion rot = Quaternion.Euler(0f,a,0f);
        DoorController d = Instantiate(doorPrefab, point, rot);
        if(d != null)
        {
            d.doorIndex = door.doorIndex;
            Color c = GameManager.GetColor(d.doorIndex);
            Colorizer colorizer = d.GetComponent<Colorizer>();
            if (colorizer != null) colorizer.UpdateColor(c);
            //if (doorColors.Count > 0)
            //{
            //    Color c = doorColors[(door.doorIndex - 1) % doorColors.Count];
            //    Colorizer colorizer = d.GetComponent<Colorizer>();
            //    if (colorizer != null) colorizer.UpdateColor(c);
            //}
        }
    }

    private void InstantiatePortal(Vector2Int coords, Vector2Int dir)
    {
        if (portalPrefabs.Count == 0) return;
        PortalController portalPrefab = portalPrefabs[UnityEngine.Random.Range(0, portalPrefabs.Count)];
        Vector3 point = new Vector3((coords.x + .5f - width / 2f) * cellSize + transform.position.x, transform.position.y, (coords.y + .5f - height / 2f) * cellSize + transform.position.z);

        float a = 0;
        if (coords.x > dir.x) a = 90f;
        else if (coords.y > dir.y) a = 180f;
        else if (coords.x < dir.x) a = 270f;
        Quaternion rot = Quaternion.Euler(0f, a, 0f);

        portal = Instantiate(portalPrefab, point, rot);
        portal.coords = coords;
    }

    private bool PlaceKey(int index, bool canBePlacedInTunnel)
    {
        if (keyPrefabs.Count == 0) return false;

        List<Vector2Int> points = new List<Vector2Int>();
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                if (cells[x, y] == null || GetExitCount(cells[x, y]) == 0) continue;
                if ( cells[x, y].roomNumber > index || (cells[x, y].roomNumber == 0 && canBePlacedInTunnel))
                {
                    Vector2Int temp = new Vector2Int(x, y);
                    if (!filledPoints.Contains(temp)) points.Add(temp);
                }
            }
        }

        if(points.Count > 0)
        {
            KeyController keyPrefab = keyPrefabs[UnityEngine.Random.Range(0, keyPrefabs.Count)];

            Vector2Int p = points[UnityEngine.Random.Range(0, points.Count)];
            Vector3 point = new Vector3((p.x + .5f - width / 2f) * cellSize + transform.position.x, transform.position.y, (p.y + .5f - height / 2f) * cellSize + transform.position.z);

            KeyController k = Instantiate(keyPrefab, point, Quaternion.identity);
            if (k != null)
            {
                k.coords = p;
                k.doorIndex = index;
                Color c = GameManager.GetColor(k.doorIndex);
                Colorizer colorizer = k.GetComponent<Colorizer>();
                if (colorizer != null) colorizer.UpdateColor(c);
                //if (doorColors.Count > 0)
                //{
                //    Color c = doorColors[(index - 1) % doorColors.Count];
                //    Colorizer colorizer = k.GetComponent<Colorizer>();
                //    if (colorizer != null) colorizer.UpdateColor(c);
                //}
            }
            filledPoints.Add(p);
            return true;
        }
        else if(!canBePlacedInTunnel)
        {
            return PlaceKey(index, true);
        }
        return false;
    }

    //private void makeExit(Vector2Int cell)
    //{
    //    if(cell.x == 0)
    //    {
    //        cells[cell.x, cell.y].hasLeftWall = false;
    //    }else if(cell.y == 0)
    //    {
    //        cells[cell.x, cell.y].hasBottomWall = false;
    //    }else if(cell.x == width - 1)
    //    {
    //        cells[cell.x, cell.y].hasRightWall = false;
    //    }
    //    else if (cell.y == height - 1)
    //    {
    //        cells[cell.x, cell.y].hasTopWall = false;
    //    }
    //}

    private void RemoveWallsWithBacktracker()
    {
        MazeCell current = cells[startPoint.x, startPoint.y];
        current.visited = true;

        Stack<MazeCell> stack = new Stack<MazeCell>();

        do
        {
            List<MazeCell> neighbours = new List<MazeCell>();
            if (!current.room)
            {
                int x = current.x;
                int y = current.y;
                if (x > 0 && !cells[x - 1, y].visited) neighbours.Add(cells[x - 1, y]);
                if (y > 0 && !cells[x, y - 1].visited) neighbours.Add(cells[x, y - 1]);
                if (x < width - 1 && !cells[x + 1, y].visited) neighbours.Add(cells[x + 1, y]);
                if (y < height - 1 && !cells[x, y + 1].visited) neighbours.Add(cells[x, y + 1]);
            }
            if(neighbours.Count > 0)
            {
                MazeCell cell = neighbours[UnityEngine.Random.Range(0, neighbours.Count)];
                RemoveWall(current, cell);
                cell.visited = true;
                stack.Push(cell);
                current = cell;
            }
            else
            {
                current = stack.Pop();
            }
        } while (stack.Count > 0);
    }

    private void MakeCycles(int count)
    {
        if (count == 0) return;

        List<MazeCell> points = new List<MazeCell>();
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                if (cells[x, y] == null || cells[x, y].room) continue;
                points.Add(cells[x, y]);

            }
        }

        while(count > 0 && points.Count > 0)
        {
            MazeCell cell = points[UnityEngine.Random.Range(0, points.Count)];
            int i = 0;
            if (cell.hasLeftWall && cell.x > 0 && !cells[cell.x - 1, cell.y].room) i+=1;
            if (cell.hasBottomWall && cell.y > 0 && !cells[cell.x, cell.y-1].room) i+=2;

            switch (i)
            {
                case 1:
                    RemoveWall(cell, cells[cell.x - 1, cell.y]);
                    count--;
                    break;
                case 2:
                    RemoveWall(cell, cells[cell.x, cell.y - 1]);
                    count--;
                    break;
                case 3:
                    RemoveWall(cell, UnityEngine.Random.Range(0, 2) == 1 ? cells[cell.x, cell.y - 1] : cells[cell.x - 1, cell.y]);
                    count--;
                    break;                
            }
            points.Remove(cell);
        }

    }

    private void RemoveDeadEnds()
    {
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                RemoveDeadEndCell(cells[x, y]);
            }
        }
    }

    private MazeCell RemoveDeadEndCell(MazeCell cell)
    {
        if (cell == null || GetExitCount(cell) != 1 || cell.room || (cell.x == startPoint.x && cell.y == startPoint.y) || (cell.x == exitPoint.x && cell.y == exitPoint.y))
        {
            return null;
        }
        else
        {
            int x = cell.x;
            int y = cell.y;
            if (!cell.hasBottomWall && y > 0)
            {
                cell.hasBottomWall = true;
                cells[x, y - 1].hasTopWall = true;

                return RemoveDeadEndCell(cells[x, y - 1]);
            }
            else if (!cell.hasTopWall && y < height - 1)
            {
                cell.hasTopWall = true;
                cells[x, y + 1].hasBottomWall = true;
                return RemoveDeadEndCell(cells[x, y + 1]);
            }
            else if (!cell.hasLeftWall && x > 0)
            {
                cell.hasLeftWall = true;
                cells[x - 1, y].hasRightWall = true;
                return RemoveDeadEndCell(cells[x - 1, y]);
            }
            else if (!cell.hasRightWall && x < width - 1)
            {
                cell.hasRightWall = true;
                cells[x + 1, y].hasLeftWall = true;
                return RemoveDeadEndCell(cells[x + 1, y]);
            }
            else return null;
        }
    }

    private void RemoveWall(MazeCell a, MazeCell b)
    {
        if(a.locked || b.locked)
        {
            MazeDoor door = new MazeDoor();
            door.cell1 = a;
            door.cell2 = b;
            if (b.roomNumber == 0) door.doorIndex = a.roomNumber;
            else door.doorIndex = b.roomNumber;
            doors.Add(door);
        }
        if (a.x == b.x)
        {
            if (a.y > b.y)
            {
                a.hasBottomWall = false;
                b.hasTopWall = false;
            }
            else
            {
                b.hasBottomWall = false;
                a.hasTopWall = false;
            }
        }
        else
        {
            if (a.x > b.x)
            {
                a.hasLeftWall = false;
                b.hasRightWall = false;
            }
            else
            {
                b.hasLeftWall = false;
                a.hasRightWall = false;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 center = transform.position + Vector3.up * cellSize / 2;
        Gizmos.color = new Color(0,0,0,0.5f);
        Gizmos.DrawCube(center, new Vector3(width * cellSize, cellSize, height * cellSize));
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        center = new Vector3((startPoint.x + .5f - width / 2f) * cellSize + transform.position.x, transform.position.y + cellSize / 2, (startPoint.y + .5f - height / 2f) * cellSize + transform.position.z);
        Gizmos.DrawCube(center, new Vector3(cellSize, cellSize, cellSize));

        center = new Vector3((exitPoint.x + .5f - width / 2f) * cellSize + transform.position.x, transform.position.y + cellSize / 2, (exitPoint.y + .5f - height / 2f) * cellSize + transform.position.z);
        Gizmos.DrawCube(center, new Vector3(cellSize, cellSize, cellSize));
        if (cells == null) return;
        Gizmos.color = new Color(1, 1, 0, 0.5f);
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                if (cells[x, y] == null) continue;
                if (!cells[x, y].visited)
                {
                    center = new Vector3((x + .5f - width / 2f) * cellSize + transform.position.x, transform.position.y + cellSize, (y + .5f - height / 2f) * cellSize + transform.position.z);
                    Gizmos.DrawCube(center, new Vector3(cellSize, cellSize*2, cellSize));
                }
            }
        }
    }
}
