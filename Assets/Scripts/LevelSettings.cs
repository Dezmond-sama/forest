using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSettings 
{
    public List<EnemyController> enemyPrefabs;
    public int enemyCount = 5;
    public int labyrinthEnemyCount = 5;


    public float cellSize = 1;

    public int width = 20;
    public int height = 15;

    public Vector2Int playerSpawn;

    public int labyrinthWidth;
    public int labyrinthHeight;
    public float labyrinthCellSize = 1;
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
    
    public List<MazeElement> cellPrefabs;
    public List<MazeCellPrefab> cellPrefabsWithWalls;
    public List<MazeCellPrefab> roomElemPrefabs;
    public List<LevelElement> roomConnectorPrefabs;
    public List<LevelElement> wallCornerPrefabs;
        
    public Vector2Int startPoint;
    public Vector2Int exitPoint;
}
