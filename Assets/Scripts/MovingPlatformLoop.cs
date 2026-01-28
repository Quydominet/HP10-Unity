using UnityEngine;

public class MovingPlatformLoop : MonoBehaviour
{
    public Transform[] points;
    public float speed = 2f;

    private int currentPointIndex = 0;

    void Update()
    {
        if (points.Length == 0) return;

        Transform target = points[currentPointIndex];

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            currentPointIndex++;

            if (currentPointIndex >= points.Length)
            {
                currentPointIndex = 0; // loop back
            }
        }
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
