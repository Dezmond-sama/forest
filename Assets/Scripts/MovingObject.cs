using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : LevelElement
{
    public Vector2Int coords;
    public float speed = 3f;
    public AnimationCurve moveCurve;
    public AnimationCurve attackCurve;

    public MovingDirection lookDirection;

    [HideInInspector]
    public bool isRotating = false;
    [HideInInspector]
    public bool isMoving = false;

    private bool isPlayer;
    [HideInInspector]
    public bool isVisible = true;

    public bool IsPlayer
    {
        get => isPlayer;
        private set => isPlayer = value;
    }
    private void Start()
    {
        isPlayer = GetComponent<PlayerController>() != null;
    }


    private void OnBecameVisible()
    {
        isVisible = true;
    }
    private void OnBecameInvisible()
    {
        isVisible = false;
    }
}
