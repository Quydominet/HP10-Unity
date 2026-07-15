using UnityEngine;

public class PauseGameplay : MonoBehaviour
{
    [SerializeField] private GameObject menuCanvas;

    void Update()
    {
        // Kiểm tra xem người chơi có bấm nút Escape (ESC) không
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        if (menuCanvas != null)
        {
            // Đảo ngược trạng thái hiện tại (Đang bật -> Tắt, Đang tắt -> Bật)
            bool isActive = menuCanvas.activeSelf;
            menuCanvas.SetActive(!isActive);

            // Tùy chọn: Khóa hoặc mở khóa chuột khi menu hiện lên
            if (!isActive)
            {
                Cursor.lockState = CursorLockMode.None; // Hiện chuột để bấm nút menu
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked; // Ẩn và khóa chuột lại vào tâm màn hình (nếu là game FPS)
                Cursor.visible = false;
            }
        }
    }
}
