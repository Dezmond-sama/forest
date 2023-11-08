using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalController : MonoBehaviour
{
    public Vector2Int coords;
    public GameObject portalElem;
    public bool isOpen = false;

    public void OpenPortal()
    {
        if (isOpen) return;
        isOpen = true;
        portalElem.SetActive(true);
    }
}
