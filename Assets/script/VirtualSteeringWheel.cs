using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualSteeringWheel : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Настройки руля")]
    public float maxRotationAngle = 120f; // Максимальный угол поворота в градусах (в одну сторону)
    public float returnSpeed = 5f; // Скорость возврата руля в центр, если отпустить палец

    private RectTransform wheelRect;
    private float currentAngle = 0f;
    private bool isDragging = false;
    private float previousAngle = 0f;

    void Start()
    {
        wheelRect = GetComponent<RectTransform>();
    }

    void Update()
    {
        // Плавный возврат руля в центр, если игрок отпустил палец
        if (!isDragging && currentAngle != 0f)
        {
            currentAngle = Mathf.Lerp(currentAngle, 0f, Time.deltaTime * returnSpeed);
            
            // Если угол стал совсем маленьким, обнуляем его
            if (Mathf.Abs(currentAngle) < 0.1f) currentAngle = 0f;
            
            ApplyRotation();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        
        // Запоминаем угол, под которым палец коснулся руля
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
            // Вычисляем текущий угол пальца относительно центра руля
            float currentFingerAngle = Mathf.Atan2(localPos.y, localPos.x) * Mathf.Rad2Deg;
            
            // Находим разницу между прошлым и текущим углом
            float deltaAngle = Mathf.DeltaAngle(previousAngle, currentFingerAngle);
            
            // Прибавляем разницу к текущему углу руля
            currentAngle += deltaAngle;
            
            // Ограничиваем поворот (чтобы руль не крутился бесконечно)
            currentAngle = Mathf.Clamp(currentAngle, -maxRotationAngle, maxRotationAngle);
            
            // Применяем поворот к картинке
            ApplyRotation();
            
            // Обновляем предыдущий угол
            previousAngle = currentFingerAngle;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    private void ApplyRotation()
    {
        // Крутим картинку по оси Z
        wheelRect.localRotation = Quaternion.Euler(0, 0, currentAngle);
    }

    // ЭТОТ МЕТОД ЧИТАЕТ МАШИНА (возвращает от -1 до 1)
    public float GetSteeringValue()
    {
        return -currentAngle / maxRotationAngle;
    }
}