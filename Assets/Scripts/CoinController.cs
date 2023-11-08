using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinController : Collectable
{
    public int value = 1;

    public override void Collect(PlayerController collector)
    {
        Debug.Log("Coin Collected");
        collector.score += value;
        base.Collect(collector);
    }
}
