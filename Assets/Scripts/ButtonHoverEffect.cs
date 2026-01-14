using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalScale;
    public float scaleFactor = 1.2f; // Độ phóng to
    public float speed = 0.1f;      // Tốc độ biến đổi

    void Start()
    {
        originalScale = transform.localScale;
    }

    // Khi chuột rê vào
    public void OnPointerEnter(PointerEventData eventData)
    {
        StopAllCoroutines(); // Dừng các hiệu ứng cũ nếu có
        transform.localScale = originalScale * scaleFactor;
        transform.SetAsLastSibling();
    }

    // Khi chuột rời khỏi
    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;
    }
}