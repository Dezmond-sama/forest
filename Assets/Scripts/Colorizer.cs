using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Colorizer : MonoBehaviour
{
    public Renderer coloredElem;
    public Color col = Color.gray;
    // Start is called before the first frame update
    void Start()
    {
        //mat = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    public void UpdateColor(Color color)
    {
        col = color;
        if(coloredElem != null) coloredElem.material.color = col;
    }
}
