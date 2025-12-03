using UnityEngine;

public class Bullet : MonoBehaviour
{
   public float Life = 3;


   void Awake()
   {
    Destroy(gameObject, Life);
   }


   void OnCollisionEnter(Collision collision)
   {
    Destroy(collision.gameObject);
    Destroy(gameObject);
   }
}
