using UnityEngine;

public class queo : MonoBehaviour
{
    [Header("Car Settings")]
    public float speed = 10f;          // tốc độ chạy
    public float turnSmooth = 5f;      // mượt mà khi xoay bánh

    [Header("Car Transforms")]
    public Transform wheelFrontLeft;
    public Transform wheelFrontRight;
    public Transform carBody;          // thân xe để di chuyển


    void Update()
    {
        HandleSteering();
        MoveCar();
    }

    void HandleSteering()
    {
        float horizontal = Input.GetAxis("Horizontal");

    }

    void MoveCar()
    {
        float vertical = Input.GetAxis("Vertical");

        // Di chuyển xe theo hướng forward
        carBody.Translate(Vector3.forward * vertical * speed * Time.deltaTime);
    }
}
