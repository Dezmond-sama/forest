using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyController : Collectable
{
    public int doorIndex = 0;
    
    public override void Collect(PlayerController collector)
    {
        if (!collector.doorKeys.Contains(doorIndex)) collector.doorKeys.Add(doorIndex);
        Debug.Log("Key Collected");
        base.Collect(collector);
    }
}
