using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : LevelElement
{
    public Vector2Int coords;
    public GameObject collectEffect;
    private void Start()
    {
        if (GameManager.collectableItems == null) GameManager.collectableItems = new List<Collectable>();
        GameManager.collectableItems.Add(this);
    }
    public virtual void Collect(PlayerController collector)
    {
        if(collectEffect!=null)Instantiate(collectEffect, transform.position, transform.rotation);
        Destroy(gameObject);
    }
    private void OnDestroy()
    {
        if (GameManager.collectableItems != null) GameManager.collectableItems.Remove(this);
    }
}
