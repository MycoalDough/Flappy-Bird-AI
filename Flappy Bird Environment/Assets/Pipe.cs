using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pipe : MonoBehaviour
{
    public float moveSpeed;
    public Transform maxPipe;
    public Transform minPipe;
    public Transform midPoint;
    // Update is called once per frame

    private void Awake()
    {
        StartCoroutine(killmyself());
    }
    void Update()
    {
        transform.position = new Vector2(transform.position.x - moveSpeed, transform.position.y);
    }

    IEnumerator killmyself()
    {
        yield return new WaitForSeconds(40);
        Destroy(gameObject);
    }
}
