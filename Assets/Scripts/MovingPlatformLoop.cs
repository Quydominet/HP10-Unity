using UnityEngine;
using System.Collections;

public class MovingPlatformLoop : MonoBehaviour
{
    public Transform[] points;
    public float speed = 2f;
    public float waitTime = 1.5f; // cooldown at each point

    private int currentPointIndex = 0;
    private bool isWaiting = false;

    void Update()
    {
        if (points.Length == 0 || isWaiting) return;

        Transform target = points[currentPointIndex];

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            StartCoroutine(WaitThenMoveNext());
        }
    }

    IEnumerator WaitThenMoveNext()
    {
        isWaiting = true;

        yield return new WaitForSeconds(waitTime);

        currentPointIndex++;
        if (currentPointIndex >= points.Length)
            currentPointIndex = 0;

        isWaiting = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}
