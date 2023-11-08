using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Vector3 offset;
    public Transform target;
    public float smoothTime = 5f;
    private Vector3 currentVelocity;

    //private Camera theCamera;

    //public BoxCollider bounds;
    //public LayerMask boundsLayer;

    //public Vector3 topLeftBound, bottomRightBound;

    //public Vector3 shiftTop;
    //public Vector3 shiftBottom;

    // Start is called before the first frame update
    void Start()
    {
        if (target == null)
        {
            PlayerController pc = FindObjectOfType<PlayerController>();
            if (pc != null) target = pc.transform;
        }
    }
    void LateUpdate()
    {
        transform.position = Vector3.SmoothDamp(transform.position, target.position + offset, ref currentVelocity, smoothTime);        
    }
    public void UpdatePosition()
    {
        transform.position = target.position + offset;
    }
}
