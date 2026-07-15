using UnityEngine;

public class FloatingObstacle : MonoBehaviour
{
    [Header("Floating")]
    public float floatHeight = 1.5f;
    public float floatSpeed = 2f;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float changeDirectionTime = 3f;

    private Vector3 startPos;
    private Vector3 moveDir;
    private float timer;
    private float offset;

    void Start()
    {
        startPos = transform.position;
        moveDir = Random.onUnitSphere;
        moveDir.y = 0; // không bay chéo lên trời
        offset = Random.Range(0f, 10f);
    }

    void Update()
    {
        // 🌊 Bay lên xuống
        float y = Mathf.Sin(Time.time * floatSpeed + offset) * floatHeight;
        transform.position = new Vector3(
            transform.position.x,
            startPos.y + y,
            transform.position.z
        );

        // ➡️ Di chuyển tự do
        transform.position += moveDir * moveSpeed * Time.deltaTime;

        timer += Time.deltaTime;
        if (timer > changeDirectionTime)
        {
            moveDir = Random.onUnitSphere;
            moveDir.y = 0;
            timer = 0;
        }
    }
}
