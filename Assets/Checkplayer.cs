using UnityEngine;

public class Checkplayer : MonoBehaviour
{
    [SerializeField] string playerTag = "Player";
    [SerializeField] Transform mover;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals(playerTag))
        {
            other.gameObject.transform.parent = mover;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Equals(playerTag))
        {
            other.gameObject.transform.parent = null;
        }
    }

}
