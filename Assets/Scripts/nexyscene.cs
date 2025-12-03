using UnityEngine;
using UnityEngine.SceneManagement;

public class NextScene : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "SampleScene"; 

    private void OnTriggerEnter(Collider other)
    { 
print("Hel!");
        if (other.CompareTag("Player"))
        { print("Hello, this is a test!");
            Debug.Log("Player entered teleporter. Loading scene: " + sceneToLoad);
            print("Hello, this ");
            SceneManager.LoadScene(sceneToLoad);
            print("gay");
        }
    }
}
