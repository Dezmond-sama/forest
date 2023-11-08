using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Heart : MonoBehaviour
{
    public Image img;
    public Sprite heart, heartEmpty;

    public void SetFull(bool b)
    {
        img.sprite = b ? heart : heartEmpty;
    }

}
