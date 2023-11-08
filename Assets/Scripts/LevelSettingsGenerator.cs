using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LevelSettingsGenerator : MonoBehaviour
{
    public float cellSize = 1;

    public bool createOuterWalls = true;
    public bool singleWalls = false;


    public Vector2Int minSize = new Vector2Int(5, 5), maxSize = new Vector2Int(25, 20);
    public float sizeCoef = 1.1f;

    public Vector2Int minimumRoomSize = new Vector2Int(2, 2);
    public Vector2Int maximumRoomSize = new Vector2Int(5, 5);

    public int minEnemyCount = 5, maxEnemyCount = 50;
    public float enemyCountRandomizeCoef = .2f;
    public float enemyCountCoef = 1.2f;

    public int minLabyrinthEnemyCount = 5, maxLabyrinthEnemyCount = 50;
    public float labyrinthEnemyCountRandomizeCoef = .2f;
    public float labyrinthEnemyCountCoef = 1.2f;

    public int minRoomCount = 0, maxRoomCount = 1;
    public float maxRoomCountCoef = 1.2f;
    public int minLockedCount = 0, maxLockedCount = 10;

    public int minCoinsInLockedRoomCount = 0, maxCoinsInLockedRoomCount = 10;
    public int minCoinsInOpenCount = 0, maxCoinsInOpenCount = 40;

    public int tryingPlaceRoomIterations = 5;

    public int minCyclesCount = 0, maxCyclesCount = 10;
    public float removeDeadEndsChance = 1;
    public float removeDeadEndsCoef = .9f;

    public List<EnemyController> enemyPrefabs;

    public List<DoorController> doorPrefabs;
    public ShadowBlock shadowBlock;
    public List<KeyController> keyPrefabs;
    public List<Collectable> coinPrefabs;

    public List<MazeElement> cellPrefabs;
    public List<MazeCellPrefab> cellPrefabsWithWalls;
    public List<MazeCellPrefab> roomElemPrefabs;
    public List<LevelElement> roomConnectorPrefabs;
    public List<LevelElement> wallCornerPrefabs;

    public LevelSettings GetSettings(int level, Vector2Int startPoint)
    {
        LevelSettings settings = new LevelSettings();

        Debug.Log(settings);
        settings.createOuterWalls = createOuterWalls;
        settings.singleWalls = singleWalls;

        settings.cellSize = cellSize;

        settings.width = Mathf.RoundToInt(Mathf.Clamp(minSize.x * Mathf.Pow(sizeCoef, (level - 1)), minSize.x, maxSize.x));
        settings.height = Mathf.RoundToInt(Mathf.Clamp(minSize.y * Mathf.Pow(sizeCoef, (level - 1)), minSize.y, maxSize.y));

        settings.labyrinthWidth = settings.width;
        settings.labyrinthHeight = settings.height;
        settings.labyrinthCellSize = settings.cellSize;

        float e = Mathf.Clamp(minEnemyCount * Mathf.Pow(enemyCountCoef, (level - 1)), minEnemyCount, maxEnemyCount);
        e = Random.Range(e * (1 - enemyCountRandomizeCoef), e * (1 + enemyCountRandomizeCoef));
        settings.enemyCount = Mathf.RoundToInt(e);

        e = Mathf.Clamp(minLabyrinthEnemyCount * Mathf.Pow(labyrinthEnemyCountCoef, (level - 1)), minLabyrinthEnemyCount, maxLabyrinthEnemyCount);
        e = Random.Range(e * (1 - labyrinthEnemyCountRandomizeCoef), e * (1 + labyrinthEnemyCountRandomizeCoef));
        settings.labyrinthEnemyCount = Mathf.RoundToInt(e);

        settings.minimumRoomSize = minimumRoomSize;
        settings.maximumRoomSize = maximumRoomSize;

        settings.roomCount = Random.Range(minRoomCount, Mathf.RoundToInt(maxRoomCount * Mathf.Pow(maxRoomCountCoef, (level - 1))));

        settings.lockedCount = Random.Range(minLockedCount, Mathf.Clamp(maxLockedCount, 0, settings.roomCount) + 1);

        settings.coinsInOpenCount = Random.Range(minCoinsInOpenCount, Mathf.Min(maxCoinsInOpenCount + 1, (settings.width * settings.height) / 3));
        settings.coinsInLockedCount = Random.Range(minCoinsInLockedRoomCount * settings.lockedCount, maxCoinsInLockedRoomCount * settings.lockedCount);

        settings.cyclesCount = Random.Range(minCyclesCount, maxCyclesCount + 1);

        float de = Mathf.Clamp(removeDeadEndsChance * Mathf.Pow(removeDeadEndsCoef, (level - 1)), 0, 1);
        settings.removeDeadEnds = Random.Range(0f, 1f) < de;

        settings.enemyPrefabs = enemyPrefabs;
        settings.doorPrefabs = doorPrefabs;
        settings.shadowBlock = shadowBlock;
        settings.keyPrefabs = keyPrefabs;
        settings.coinPrefabs = coinPrefabs;
        settings.cellPrefabs = cellPrefabs;
        settings.cellPrefabsWithWalls = cellPrefabsWithWalls;
        settings.roomElemPrefabs = roomElemPrefabs;
        settings.roomConnectorPrefabs = roomConnectorPrefabs;
        settings.wallCornerPrefabs = wallCornerPrefabs;

        settings.startPoint = startPoint;
        settings.playerSpawn = startPoint;

        settings.tryingPlaceRoomIterations = tryingPlaceRoomIterations;

        settings.exitPoint = new Vector2Int(Random.Range(0, settings.width), Random.Range(0, settings.height));

        return settings;
    }
}
