using UnityEngine;

public class ExitGames : MonoBehaviour
{

    public void ExitGame()
    {
        Debug.Log("Game đang thoát...");
        Application.Quit();

        #if UNITY_EDITOR

        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
