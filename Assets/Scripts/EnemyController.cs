using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MovingObject
{
    public int damage = 1;
    public bool alwaysWalk = false;

    public int stepCount = 1;

    [HideInInspector]
    public int stepCounter = 1;

    private void Start()
    {
        if (GameManager.enemies == null) GameManager.enemies = new List<EnemyController>();
        GameManager.enemies.Add(this);
        stepCounter = stepCount;
        Debug.Log("Create Enemy");
        GameManager.UpdateUI();
    }

    private void OnDestroy()
    {
        GameManager.enemies.Remove(this);
        GameManager.UpdateUI();
    }
}
