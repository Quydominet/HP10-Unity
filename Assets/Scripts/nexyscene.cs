using UnityEngine;
using UnityEngine.SceneManagement;

public class NextScene : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "SampleScene";

    private bool startTriggered = false;
    private bool antiCheeseTriggered = false;

    // Called by AntiCheese object
    public void AntiCheeseTouched()
    {
        antiCheeseTriggered = true;
        Debug.Log("Anti-cheese completed");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // First visit = START
        if (!startTriggered)
        {
            startTriggered = true;
            Debug.Log("GAY");
            return;
        }

        // Second visit = TELEPORT (only if anti-cheese done)
        if (startTriggered && antiCheeseTriggered)
        {
            Debug.Log("YAOURI");
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.Log("FUCK YOU");
        }
    }
}
