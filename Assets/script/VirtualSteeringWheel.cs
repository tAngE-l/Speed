using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualSteeringWheel : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Настройки руля")]
    public float maxRotationAngle = 120f; 
    public float returnSpeed = 5f; 
    
    [Header("Чувствительность (меняется из UIManager)")]
    public float tiltSensitivity = 1.0f; // Множитель угла

    private RectTransform wheelRect;
    private float currentAngle = 0f;
    private bool isDragging = false;
    private float previousAngle = 0f;

    void Start()
    {
        wheelRect = GetComponent<RectTransform>();
        wheelRect.pivot = new Vector2(0.5f, 0.5f);
    }

    void Update()
    {
        if (!isDragging && currentAngle != 0f)
        {
            currentAngle = Mathf.Lerp(currentAngle, 0f, Time.deltaTime * returnSpeed);
            if (Mathf.Abs(currentAngle) < 0.1f) currentAngle = 0f;
            ApplyRotation();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        Vector2 localPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(wheelRect, eventData.position, eventData.pressEventCamera, out localPos))
        {
            previousAngle = Mathf.Atan2(localPos.y, localPos.x) * Mathf.Rad2Deg;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(wheelRect, eventData.position, eventData.pressEventCamera, out localPos))
        {
            float currentFingerAngle = Mathf.Atan2(localPos.y, localPos.x) * Mathf.Rad2Deg;
            float deltaAngle = Mathf.DeltaAngle(previousAngle, currentFingerAngle);
            
            // Применяем чувствительность
            currentAngle += deltaAngle * tiltSensitivity; 
            
            currentAngle = Mathf.Clamp(currentAngle, -maxRotationAngle, maxRotationAngle);
            ApplyRotation();
            previousAngle = currentFingerAngle;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    private void ApplyRotation()
    {
        wheelRect.localRotation = Quaternion.Euler(0, 0, currentAngle);
    }

    public float GetSteeringValue()
    {
        return -currentAngle / maxRotationAngle; 
    }
}