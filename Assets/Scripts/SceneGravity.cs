using UnityEngine;

public class SceneGravity : MonoBehaviour
{
    void Start()
    {
        // Trọng lực mặc định toàn scene
        Physics.gravity = new Vector3(0, -9.81f, 0);

        Debug.Log("Gravity Applied To Whole Scene!");
    }
}
