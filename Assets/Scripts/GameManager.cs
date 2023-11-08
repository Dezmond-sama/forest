using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum MovingDirection  {moveLeft, moveTop, moveRight, moveBottom };
public enum CellStatus {walkableCell, unwalkableCell, enemyCell, playerCell,locked};
public enum GameState { loading, playerTurn, enemyTurn,gameOver}
[RequireComponent(typeof(GameField))]
public class GameManager : MonoBehaviour
{
    private PlayerController player;
    private CameraController theCamera;
    public static List<EnemyController> enemies = new List<EnemyController>();
    public static List<Collectable> collectableItems = new List<Collectable>();
    public static List<ShadowBlock> shadowBlockList = new List<ShadowBlock>();
    private GameField field;
    private Maze[] mazes;
    //private bool isMoving = false;
    //private bool isRotating = false;
    public LayerMask ground;
    public GameState turn;

    private Vector3 startMousePoint, mouseVector;
    public int mouseThreshold = 100;

    private int walkingEnemy = 0;

    public Text scoreLabel;
    public Text enemiesLabel;
    public Text levelLabel;
    public GridLayoutGroup keysGrid;
    public GridLayoutGroup heartsGrid;

    public Image keyImage;
    public Image heartImage;
    private static GameManager instance;

    public List<Color> doorColors = new List<Color>();
    public List<KeySprite> keySprites = new List<KeySprite>();

    public List<Heart> heartSprites = new List<Heart>();

    public LevelSettingsGenerator settingsGenerator;

    public int seed = 0;

    int level = 0;

    public GameObject gameOverPanel;

    public PortalController portal = null;

    public string menuScene;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start loading");
        turn = GameState.loading;

        gameOverPanel.SetActive(false);
        UnityEngine.Random.InitState(seed);
        LevelSettings settings;

        level = PlayerPrefs.GetInt("CurrentLevel",1);
        Vector2Int startPoint = new Vector2Int(PlayerPrefs.GetInt("StartX", 1), PlayerPrefs.GetInt("StartY", 1));
        instance = this;

        if (settingsGenerator != null)
        {
            settings = settingsGenerator.GetSettings(level, startPoint);
            Debug.Log(settings);
        }
        else
        {
            settings = null;
        }

        player = FindObjectOfType<PlayerController>();
        player.doorKeys.Clear();
        player.score = PlayerPrefs.GetInt("CurrentScore", 0);
        player.currentHP = PlayerPrefs.GetInt("CurrentHP", player.currentHP);
        player.maxHP = PlayerPrefs.GetInt("MaxHP", player.maxHP);

        mazes = FindObjectsOfType<Maze>();
        foreach (Maze maze in mazes)
        {
            if (settings != null)
            {
                maze.initSettings(settings,level);
            }
            maze.initMaze();
            if (maze.portal != null) portal = maze.portal;
        }
        
        field = GetComponent<GameField>();
        if (settings != null) field.InitSettings(settings, level);
        field.Init();
        
        Spawn(player.transform, field.cells[field.playerSpawnX, field.playerSpawnY].coords);

        theCamera = FindObjectOfType<CameraController>();
        theCamera.target = player.transform;
        theCamera.UpdatePosition();


        MovingObject m = player.GetComponent<MovingObject>();
        player.coords.x = field.playerSpawnX;
        player.coords.y = field.playerSpawnY;
        turn = GameState.playerTurn;
        Debug.Log("Loaded");
    }


    // Update is called once per frame
    void LateUpdate()
    {
        Debug.Log(walkingEnemy);
        if (turn == GameState.gameOver || turn == GameState.loading) return;

        if (enemies.Count == 0 && portal != null && !portal.isOpen)
        {
            OpenExit();
            portal.OpenPortal();
            CheckPortal();
        }

        if (player.isMoving || player.isRotating) return;
        
        if (turn == GameState.playerTurn)
        {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");

            if (Input.GetMouseButtonDown(0))
            {
                startMousePoint = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(0))
            {
                mouseVector = Input.mousePosition - startMousePoint;
                x = 0;
                y = 0;
                if (mouseVector.x > mouseThreshold) x = 1;
                else if (mouseVector.x < -mouseThreshold) x = -1;
                else if (mouseVector.y > mouseThreshold) y = 1;
                else if (mouseVector.y < -mouseThreshold) y = -1;
            }

            
            //if (y > 0.1f) MoveObject(player, player.lookDirection);
            //else if (y < -0.1f)
            //{
            //    int r = (int)(player.lookDirection + 2) % 4;
            //    MoveObject(player, (MovingDirection)(r));
            //}
            //else if (x > 0.1f) StartCoroutine(RotateCoroutine(player, 1));
            //else if (x < -0.1f) StartCoroutine(RotateCoroutine(player, -1));
            bool b = false;
            if (x > 0.1f) b = MoveObject(player, MovingDirection.moveRight);
            else if (x < -0.1f) b = MoveObject(player, MovingDirection.moveLeft);
            else if (y > 0.1f) b = MoveObject(player, MovingDirection.moveTop);
            else if (y < -0.1f) b = MoveObject(player, MovingDirection.moveBottom);
            if (b && enemies.Count > 0)
            {                
                turn = GameState.enemyTurn;
            }
        }
        else if(turn == GameState.enemyTurn)
        {
            if(walkingEnemy >= enemies.Count)
            {
                if (enemies.Count>0 && (enemies[enemies.Count - 1].isMoving || enemies[enemies.Count - 1].isRotating)) return;
                turn = GameState.playerTurn;
                walkingEnemy = 0;
            }
            else
            {
                if (walkingEnemy > 0 && (enemies[walkingEnemy - 1].isMoving || enemies[walkingEnemy - 1].isRotating)) return;
                if (enemies[walkingEnemy].isMoving || enemies[walkingEnemy].isRotating) return;
                int d = UnityEngine.Random.Range(0, 4);
                bool b = MoveObject(enemies[walkingEnemy], (MovingDirection)d);
                if (enemies[walkingEnemy].alwaysWalk)
                {
                    int iter = 4;
                    while (!b && iter > 0)
                    {
                        d += 1;
                        d = d % 4;
                        b = MoveObject(enemies[walkingEnemy], (MovingDirection)d);
                        iter--;
                    }
                }
                enemies[walkingEnemy].stepCounter--;
                if (enemies[walkingEnemy].stepCounter <= 0)
                {
                    enemies[walkingEnemy].stepCounter = enemies[walkingEnemy].stepCount;
                    walkingEnemy++;
                }
            }
        }

    }

    private void OpenExit()
    {
        Debug.Log("OpenExit");
        for (int x = 0; x < field.width - 1; ++x)
        {
            for (int y = 0; y < field.height - 1; ++y)
            {
                if (field.cells[x, y].bottomCell != null && field.cells[x, y].bottomCell.door != null && field.cells[x, y].bottomCell.door.doorIndex == -1) field.cells[x, y].bottomCell.door.OpenDoor();
                if (field.cells[x, y].topCell != null && field.cells[x, y].topCell.door != null && field.cells[x, y].topCell.door.doorIndex == -1) field.cells[x, y].topCell.door.OpenDoor();
                if (field.cells[x, y].leftCell != null && field.cells[x, y].leftCell.door != null && field.cells[x, y].leftCell.door.doorIndex == -1) field.cells[x, y].leftCell.door.OpenDoor();
                if (field.cells[x, y].rightCell != null && field.cells[x, y].rightCell.door != null && field.cells[x, y].rightCell.door.doorIndex == -1) field.cells[x, y].rightCell.door.OpenDoor();
            }
        }        
    }

    private void CheckPortal()
    {
        if (portal == null || !portal.isOpen) return;
        
        if (portal.coords == player.coords)
        {
            level++;

            PlayerPrefs.SetInt("CurrentLevel", level);
            PlayerPrefs.SetInt("StartX", player.coords.x);
            PlayerPrefs.SetInt("StartY", player.coords.y);
            PlayerPrefs.SetInt("CurrentScore", player.score);
            PlayerPrefs.SetInt("CurrentHP", player.currentHP);
            PlayerPrefs.SetInt("MaxHP", player.maxHP);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        
    }

    private void CheckCollectable()
    {
        if (collectableItems == null) return;
        foreach (Collectable coll in collectableItems)
        {
            if (coll.coords == player.coords)
            {
                coll.Collect(player);
                UpdateUI();
                break;
            }
        }
    }

    void Spawn(Transform obj, Vector3 coords)
    {
        obj.position = coords;
    }

    bool MoveObject(MovingObject obj, MovingDirection direction)
    {
        GameFieldCell cell = field.cells[obj.coords.x, obj.coords.y];
        GameFieldCellLink destCell = null;
        switch (direction)
        {
            case MovingDirection.moveLeft:
                destCell = cell.leftCell;                
                break;
            case MovingDirection.moveRight:
                destCell = cell.rightCell;
                break;
            case MovingDirection.moveTop:
                destCell = cell.topCell;
                break;
            case MovingDirection.moveBottom:
                destCell = cell.bottomCell;
                break;
        }

        if (destCell != null)
        {
            Debug.Log(CheckCellLink(destCell));

            switch (CheckCellLink(destCell))
            {
                case CellStatus.locked:
                    if (obj.IsPlayer && destCell.door != null && player.doorKeys.Contains(destCell.door.doorIndex))
                    {
                        StartCoroutine(OpenDoorCoroutine(obj, cell, destCell.cell, destCell.door, direction));
                        player.doorKeys.Remove(destCell.door.doorIndex);
                        UpdateUI();
                        return true;

                    }
                    break;
                case CellStatus.walkableCell:
                    StartCoroutine(MoveCoroutine(obj, cell, destCell.cell, direction));
                    return true;
                case CellStatus.enemyCell:
                    if (obj.IsPlayer)
                    {
                        StartCoroutine(AttackCoroutine(obj, cell, destCell.cell, direction));
                        return true;
                    }
                    break;
                case CellStatus.playerCell:
                    if (!obj.IsPlayer)
                    {
                        StartCoroutine(AttackCoroutine(obj, cell, destCell.cell, direction));
                        return true;
                    }
                    break;
            }

        }

        return false;
    }

    CellStatus CheckCellLink(GameFieldCellLink cell)
    {
        if (cell.door != null && cell.door.isLocked) return CellStatus.locked;

        if (!cell.cell.isWalkable) return CellStatus.unwalkableCell;

        if (cell.cell.intCoords == player.coords) return CellStatus.playerCell;
        foreach (EnemyController enemy in enemies)
        {
            if (cell.cell.intCoords == enemy.coords) return CellStatus.enemyCell;
        }

        return CellStatus.walkableCell;
    }

    EnemyController GetEnemyAtPoint(GameFieldCell cell)
    {
        foreach (EnemyController enemy in enemies)
        {
            if (cell.intCoords == enemy.coords) return enemy;
        }

        return null;
    }

    IEnumerator MoveCoroutine(MovingObject movingObject, GameFieldCell startCell, GameFieldCell endCell, MovingDirection direction)
    {
        Vector3 startPoint = startCell.coords;
        Vector3 endPoint = endCell.coords;
        movingObject.isMoving = true;
        movingObject.coords = endCell.intCoords;
        while (movingObject.isRotating || movingObject.lookDirection != direction)
        {
            yield return null;
            if (movingObject == null) yield break;
            if (movingObject.isRotating) continue;
            int d = direction - movingObject.lookDirection;
            if (d >= 3) d -= 4;
            if (d <= -3) d += 4;
            //Debug.Log(d);
            if (d != 0) StartCoroutine(RotateCoroutine(movingObject, d));
        }

        RaycastHit hit;
        if (movingObject.isVisible)
        {
            float f = 0;
            float speed = movingObject.speed;
            if (speed <= 0) speed = 1;
            bool b = movingObject.IsPlayer;
            bool p = b;
            while (f < 1)
            {
                if (f > .5f && b)
                {
                    CheckCollectable();
                    b = false;
                }
                if (f > .5f && p)
                {
                    CheckPortal();
                    p = false;
                }

                if (movingObject == null) yield break;
                float t = movingObject.moveCurve.Evaluate(f);
                Vector3 point = Vector3.Lerp(startPoint, endPoint, t) + Vector3.up * 100;
                if (Physics.Raycast(point, Vector3.down, out hit, 200, ground)) point = hit.point;
                else point -= Vector3.up * 100;
                movingObject.transform.position = point;//new Vector3(point.x, movingObject.transform.position.y, point.y);
                f += Time.deltaTime * speed;
                yield return null;
            }
        }
        else if (movingObject.IsPlayer)
        {
            CheckCollectable();
            CheckPortal();
        }

        if (Physics.Raycast(endPoint + Vector3.up * 100, Vector3.down, out hit, 200, ground)) endPoint = hit.point;

        movingObject.transform.position = endPoint;//new Vector3(endPoint.x, movingObject.transform.position.y, endPoint.y);
        movingObject.isMoving = false;
    }

    IEnumerator AttackCoroutine(MovingObject movingObject, GameFieldCell startCell, GameFieldCell endCell, MovingDirection direction)
    {
        Vector3 startPoint = startCell.coords;
        Vector3 attackdPoint = endCell.coords;
        movingObject.isMoving = true;

        while (movingObject.isRotating || movingObject.lookDirection != direction)
        {
            yield return null;
            if (movingObject == null) yield break;
            if (movingObject.isRotating) continue;
            int d = direction - movingObject.lookDirection;
            if (d >= 3) d -= 4;
            if (d <= -3) d += 4;
            //Debug.Log(d);
            if (d != 0) StartCoroutine(RotateCoroutine(movingObject, d));
        }
        
        float f = 0;
        float speed = movingObject.speed;
        if (speed <= 0) speed = 1;
        RaycastHit hit;
        while (f < 1)
        {
            if (movingObject == null) yield break;
            float t = movingObject.attackCurve.Evaluate(f);
            Vector3 point = Vector3.Lerp(startPoint, attackdPoint, t) + Vector3.up * 100;
            if (Physics.Raycast(point, Vector3.down, out hit, 200, ground)) point = hit.point;
            else point -= Vector3.up * 100;
            movingObject.transform.position = point;//new Vector3(point.x, movingObject.transform.position.y, point.y);
            f += Time.deltaTime * speed;
            yield return null;
        }
        if (Physics.Raycast(startPoint + Vector3.up * 100, Vector3.down, out hit, 200, ground)) startPoint = hit.point;

        if (movingObject.IsPlayer)
        {
            EnemyController enemy = GetEnemyAtPoint(endCell);
            if (enemy != null)
            {
                //enemies.Remove(enemy);
                Destroy(enemy.gameObject);
            }

            if (enemies.Count == 0 && portal != null && !portal.isOpen)
            {
                OpenExit();
                portal.OpenPortal();
                CheckPortal();
            }

        }
        else
        {
            EnemyController enemy = movingObject.GetComponent<EnemyController>();
            if (enemy != null) player.hurtPlayer(enemy.damage);
            if (player.currentHP == 0) GameOver();
        }
        UpdateUI();

        movingObject.transform.position = startPoint;//new Vector3(endPoint.x, movingObject.transform.position.y, endPoint.y);
        movingObject.isMoving = false;
    }

    IEnumerator OpenDoorCoroutine(MovingObject movingObject, GameFieldCell startCell, GameFieldCell endCell, DoorController door, MovingDirection direction)
    {
        Vector3 startPoint = startCell.coords;
        Vector3 endPoint = endCell.coords;
        movingObject.isMoving = true;

        while (movingObject.isRotating || movingObject.lookDirection != direction)
        {
            yield return null;
            if (movingObject == null) yield break;
            if (movingObject.isRotating) continue;
            int d = direction - movingObject.lookDirection;
            if (d >= 3) d -= 4;
            if (d <= -3) d += 4;
            //Debug.Log(d);
            if (d != 0) StartCoroutine(RotateCoroutine(movingObject, d));
        }
        
        float f = 0;
        float speed = movingObject.speed;
        if (speed <= 0) speed = 1;
        RaycastHit hit;
        while (f < 1)
        {
            if (f > .5f && door.isLocked) door.OpenDoor();
            if (movingObject == null) yield break;
            float t = movingObject.attackCurve.Evaluate(f);
            Vector3 point = Vector3.Lerp(startPoint, endPoint, t) + Vector3.up * 100;
            if (Physics.Raycast(point, Vector3.down, out hit, 200, ground)) point = hit.point;
            else point -= Vector3.up * 100;
            movingObject.transform.position = point;//new Vector3(point.x, movingObject.transform.position.y, point.y);
            f += Time.deltaTime * speed;
            yield return null;
        }
        if (Physics.Raycast(startPoint + Vector3.up * 100, Vector3.down, out hit, 200, ground)) startPoint = hit.point;

        movingObject.transform.position = startPoint;
        movingObject.isMoving = false;
    }

    IEnumerator RotateCoroutine(MovingObject movingObject, int direction)
    {
        Quaternion startRotation = movingObject.transform.rotation;
        Quaternion endRotation = movingObject.transform.rotation * Quaternion.Euler(0f,90f*Mathf.Sign(direction),0f);
        movingObject.isRotating = true;
        if (movingObject.isVisible)
        {
            float f = 0;
            float speed = movingObject.speed;
            if (speed <= 0) speed = 1;
            while (f < 1)
            {
                if (movingObject == null) yield break;
                //float t = movingObject.moveCurve.Evaluate(f);
                Quaternion rot = Quaternion.Lerp(startRotation, endRotation, f);
                movingObject.transform.rotation = rot;
                f += Time.deltaTime * speed;
                yield return null;
            }
            movingObject.transform.rotation = endRotation;
        }
        int r = (int)(movingObject.lookDirection + (int)Mathf.Sign(direction)) % 4;
        if (r < 0) r += 4;
        movingObject.lookDirection = (MovingDirection)(r);
        
        movingObject.isRotating = false;
    }

    public static void UpdateUI()
    {
        if (instance == null) return;
        if (instance.scoreLabel != null) instance.scoreLabel.text = "x " + instance.player.score;
        if (instance.enemiesLabel != null) instance.enemiesLabel.text = "x " + enemies.Count;
        if (instance.levelLabel != null) instance.levelLabel.text = "" + instance.level;
        if (instance.keysGrid != null && instance.keyImage != null)
        {
            List<int> existingKeys = new List<int>();
            for (int i = instance.keySprites.Count - 1; i >= 0; --i)
            {
                KeySprite sprite = instance.keySprites[i];
                if (!instance.player.doorKeys.Contains(sprite.doorIndex))
                {
                    Debug.Log("remove " + sprite.doorIndex);
                    instance.keySprites.Remove(sprite);
                    Destroy(sprite.gameObject);
                }
                else
                {
                    existingKeys.Add(sprite.doorIndex);
                }
            }

            foreach (int i in instance.player.doorKeys)
            {
                if (existingKeys.Contains(i)) continue;
                Image key = Instantiate(instance.keyImage, instance.keysGrid.transform);
                key.color = GetColor(i);
                KeySprite sprite = key.GetComponent<KeySprite>();
                if (sprite != null)
                {
                    sprite.doorIndex = i;
                    instance.keySprites.Add(sprite);
                }
            }

            //newCell.transform.SetParent(this.gameObject.transform, false);
        }

        if (instance.heartsGrid != null && instance.heartImage != null)
        {
            if(instance.heartSprites.Count > instance.player.maxHP)

            for (int i = instance.heartSprites.Count - 1; i >= instance.player.maxHP; --i)
            {
                Heart sprite = instance.heartSprites[i];
                instance.heartSprites.Remove(sprite);
                Destroy(sprite.gameObject);                
            }

            for (int i = instance.heartSprites.Count; i<instance.player.maxHP; ++i)
            {
                Image heart = Instantiate(instance.heartImage, instance.heartsGrid.transform);
                Heart sprite = heart.GetComponent<Heart>();
                if (sprite != null)
                {
                    instance.heartSprites.Add(sprite);
                }
            }

            for (int i = 0; i < instance.heartSprites.Count ; ++i)
            {
                instance.heartSprites[i].SetFull(i<instance.player.currentHP);
                
            }
        }
    }
    public static Color GetColor(int index)
    {
        if (index <= 0) return Color.white;
        if (instance == null || instance.doorColors.Count == 0) return Color.white;
        else return instance.doorColors[(index - 1) % instance.doorColors.Count];
    }

    public void GameOver()
    {
        turn = GameState.gameOver;
        gameOverPanel.SetActive(true);

        PlayerPrefs.DeleteKey("CurrentLevel");
        PlayerPrefs.DeleteKey("StartX");
        PlayerPrefs.DeleteKey("StartY");
        PlayerPrefs.DeleteKey("CurrentScore");
        PlayerPrefs.DeleteKey("CurrentHP");
        PlayerPrefs.DeleteKey("MaxHP");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(menuScene);
    }
}
