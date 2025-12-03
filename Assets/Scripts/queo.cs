using UnityEngine;

public class queo : MonoBehaviour
{
    [Header("Car Settings")]
    public float speed = 10f;          // tốc độ chạy
    public float steeringAngle = 30f;  // góc xoay tối đa của bánh trước
    public float turnSmooth = 5f;      // mượt mà khi xoay bánh

    [Header("Car Transforms")]
    public Transform wheelFrontLeft;
    public Transform wheelFrontRight;
    public Transform carBody;          // thân xe để di chuyển

    float currentSteerAngle = 0f;

    void Update()
    {
        HandleSteering();
        MoveCar();
    }

    void HandleSteering()
    {
        float horizontal = Input.GetAxis("Horizontal");

        // Tính góc cần xoay
        float targetAngle = horizontal * steeringAngle;

        // Làm mượt góc xoay
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetAngle, Time.deltaTime * turnSmooth);

        // Xoay bánh trước theo trục Y
        wheelFrontLeft.localRotation = Quaternion.Euler(0, currentSteerAngle, 0);
        wheelFrontRight.localRotation = Quaternion.Euler(0, currentSteerAngle, 0);
    }

    void MoveCar()
    {
        float vertical = Input.GetAxis("Vertical");

        // Di chuyển xe theo hướng forward
        carBody.Translate(Vector3.forward * vertical * speed * Time.deltaTime);
    }
}
