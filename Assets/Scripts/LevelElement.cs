using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelElement : MonoBehaviour
{
    public int minimumLevel = 1;
    public int maximumLevel = 0;

    public bool CheckLevel(int currentLevel)
    {
        return ((currentLevel >= minimumLevel || minimumLevel == 0) && (currentLevel <= maximumLevel || maximumLevel == 0));
    }
}
