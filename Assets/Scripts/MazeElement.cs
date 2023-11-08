using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeElement : LevelElement
{
    public bool hasTopExit = true;
    public bool hasBottomExit = true;
    public bool hasLeftExit = true;
    public bool hasRightExit = true;

    public int exitCount {
        get{
            return (hasTopExit ? 1 : 0) +
                   (hasBottomExit ? 1 : 0) +
                   (hasLeftExit ? 1 : 0) +
                   (hasRightExit ? 1 : 0);
        }
    }
    public float GetRotation(bool topExit,bool bottomExit,bool leftExit,bool rightExit, int rotationCount = 0)
    {
        if (rotationCount > 4) return 0f;
        if(topExit == hasTopExit && bottomExit == hasBottomExit && leftExit == hasLeftExit && rightExit == hasRightExit)
        {
            return 0f;
        }
        else
        {
            return 90f + GetRotation(rightExit, leftExit, topExit, bottomExit, rotationCount + 1);
        }
    }
}
