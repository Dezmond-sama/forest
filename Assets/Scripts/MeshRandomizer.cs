using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshRandomizer : MonoBehaviour
{
    public List<Mesh> meshes;
    // Start is called before the first frame update
    void Start()
    {
        if (meshes.Count == 0) return;
        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter != null) filter.mesh = meshes[Random.Range(0,meshes.Count)];
    }
}
