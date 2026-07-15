using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseScript : MonoBehaviour
{
    public void Exit()
    {
        SceneManager.LoadSceneAsync(0);
    }
}
