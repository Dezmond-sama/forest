using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : MovingObject
{
    public List<int> doorKeys = new List<int>();
    public int score = 0;
    public int currentHP = 3;
    public int maxHP = 3;

    public void hurtPlayer(int damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
    }
}
