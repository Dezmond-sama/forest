using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : LevelElement
{
    public int doorIndex = 0;
    public bool isLocked = true;

    public virtual void OpenDoor()
    {
        isLocked = false;
        if (GameManager.shadowBlockList == null) return;
        foreach(ShadowBlock sh in GameManager.shadowBlockList)
        {
            if (sh.index == doorIndex) sh.DestroyBlock();
        }
        gameObject.SetActive(false);
    }
}
