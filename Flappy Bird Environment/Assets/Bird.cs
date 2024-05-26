using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Bird : MonoBehaviour
{
    public string ID;

    public float force;
    public bool hasHitPipe;
    public float distance;
    public float prev_distance;
    Pipe currentPipe;

    public float minY = -5f;
    public float maxY = 5f;
    public float minDistance = 0f;
    public float maxDistance = 30f;
    public float minVelocity = -10f;
    public float maxVelocity = 3f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            gameObject.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
            gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.up * force, ForceMode2D.Force);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        hasHitPipe = true;
    }

    public string envData()
    {
        string toReturn = "";
        float birdY = transform.position.y;
        float birdVelocityY = GetComponent<Rigidbody2D>().velocity.y;

        // Default values for no pipe detected
        float distanceToPipe = -1f;
        float maxPipeY = -1f;
        float minPipeY = -1f;
        float midPipeY = -1f;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right, 30f);
        if (hit.collider != null)
        {
            distanceToPipe = Mathf.Abs(transform.position.x - hit.collider.transform.position.x);
            distance = Mathf.Abs(transform.position.x - hit.collider.transform.position.x);
            currentPipe = hit.transform.parent.GetComponent<Pipe>();
            maxPipeY = currentPipe.maxPipe.position.y;
            minPipeY = currentPipe.minPipe.position.y;
            midPipeY = currentPipe.midPoint.position.y;
        }

        // Normalization
        float normalizedDistanceToPipe = distanceToPipe == -1f ? -1f : Normalize(distanceToPipe, minDistance, maxDistance);
        float normalizedYToPipe = maxPipeY == -1f ? -1f : Normalize(transform.position.y - currentPipe.midPoint.position.y, minY, maxY);
        float normalizedBirdVelocityY = Normalize(birdVelocityY, minVelocity, maxVelocity);

        // Construct the input string with normalized values
        toReturn = $"{normalizedYToPipe:F2}, {normalizedDistanceToPipe:F2}, {normalizedBirdVelocityY:F2}";

        return toReturn;
    }

    float Normalize(float value, float minValue, float maxValue)
    {
        return (value - minValue) / (maxValue - minValue);
    }

    public string playAction(int action)
    {
        if (action == 0)
        {

            gameObject.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
            gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.up * force, ForceMode2D.Force);
        }

        if(hasHitPipe)
        {
            return "-50:True:";
        }
        else
        {
            float reward = 0;
            if(currentPipe != null)
            {
                Debug.Log("wtf");
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right, 30f);
                if (hit.collider != null)
                {
                    float new_dist = (float)Math.Round(Math.Abs(transform.position.x - hit.collider.transform.position.x), 2);
                    if(distance < new_dist) 
                    {
                        reward += 5;
                    }
                }

               if(currentPipe != null)
               {
                    if (action == 0 && transform.position.y < currentPipe.midPoint.position.y)
                    {
                        reward += 5;
                    }
                    else if (action != 0 && transform.position.y > currentPipe.midPoint.position.y) { reward += 5; }
                    else
                    {
                        reward -= 5;
                    }
                }

                if (transform.position.y > currentPipe.minPipe.position.y && transform.position.y < currentPipe.maxPipe.position.y)
                {
                    reward += 5;
                    return reward + ":False:";
                }
                else
                {
                    reward -= 5;
                    return reward + ":False:";
                }
            }
            else
            {
                return "0:False:";
            }
            
        }
    }

    void OnDrawGizmos()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right, 30f);
        Vector2 endPosition;

        if (hit.collider != null)
        {
            endPosition = hit.point;
        }
        else
        {
            endPosition = (Vector2)transform.position + Vector2.right * 30f;
        }

        // Draw an X at the endPosition
        float xSize = 0.5f; // Size of the X mark
        Gizmos.DrawLine(endPosition - Vector2.right * xSize, endPosition + Vector2.right * xSize);
        Gizmos.DrawLine(endPosition - Vector2.up * xSize, endPosition + Vector2.up * xSize);

        if (currentPipe)
        {
            Gizmos.DrawLine(currentPipe.GetComponent<Pipe>().maxPipe.position - new Vector3(1, 0, 0), currentPipe.GetComponent<Pipe>().maxPipe.position + new Vector3(1, 0, 0));
            Gizmos.DrawLine(currentPipe.GetComponent<Pipe>().minPipe.position - new Vector3(1, 0, 0), currentPipe.GetComponent<Pipe>().minPipe.position + new Vector3(1, 0, 0));
            Gizmos.DrawLine(transform.position - new Vector3(1, 0, 0), transform.position + new Vector3(1, 0, 0));

        }
    }   
}
