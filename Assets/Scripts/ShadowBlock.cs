using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowBlock : MonoBehaviour
{
    public int index = 0;

    public float minTime = .1f;
    public float maxTime = .3f;
    private void Start()
    {
        if (GameManager.shadowBlockList == null) GameManager.shadowBlockList = new List<ShadowBlock>();
        GameManager.shadowBlockList.Add(this);
    }
    private void OnDestroy()
    {
        if (GameManager.shadowBlockList != null) GameManager.shadowBlockList.Remove(this);
    }
    public void DestroyBlock()
    {
        StartCoroutine(DeleteBlock());
    }
    IEnumerator DeleteBlock()
    {
        yield return new WaitForSeconds(Random.Range(minTime, maxTime));
        Destroy(gameObject);
    }
}
