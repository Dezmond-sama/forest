using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{

    public float speed = 100f;
    public float speedRandomzer = 50f;
    private void Start()
    {
        if (speedRandomzer != 0) speed += Random.Range(-speedRandomzer, speedRandomzer);
    }
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up, speed * Time.deltaTime);
    }
}
