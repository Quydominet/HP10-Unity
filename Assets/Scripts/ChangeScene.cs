using UnityEngine;
using UnityEngine.SceneManagement; // Thư viện quản lý Scene

public class ChangeScene : MonoBehaviour
{
    void Update()
    {
        // Kiểm tra nếu người dùng bấm phím Enter
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            LoadNextSceneIndex();
        }
    }

    void LoadNextSceneIndex()
    {
        // Lấy chỉ số index của Scene hiện tại
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Cộng thêm 1 để lấy index của Scene tiếp theo
        int nextSceneIndex = currentSceneIndex + 1;

        // Kiểm tra xem có Scene tiếp theo trong danh sách không, tránh bị lỗi vượt quá số lượng Scene
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("Đây đã là Scene cuối cùng trong danh sách Build Profiles rồi!");
        }
    }
}