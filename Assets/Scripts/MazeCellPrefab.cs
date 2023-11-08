using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeCellPrefab : LevelElement
{
    public GameObject leftWall;
    public GameObject bottomWall;
    public GameObject rightWall;
    public GameObject topWall;

    public int roomIndex = 0;
}
