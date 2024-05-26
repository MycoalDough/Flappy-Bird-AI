using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeSpawner : MonoBehaviour
{
    public GameObject pipe;

    public float time;
    public float maxTime;

    public float minY;
    public float maxY;

    private void Update()
    {
        time += Time.deltaTime;

        if(time > maxTime)
        {
            time = 0;
            Instantiate(pipe, new Vector2(15, Random.Range(minY, maxY)), Quaternion.identity);
        }
    }
}
