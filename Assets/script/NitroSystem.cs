using UnityEngine;

public class NitroSystem : MonoBehaviour
{
    [Header("Настройки")]
    public float maxCharge = 1f;
    public float chargeRate = 0.1f; // Копится само со временем
    public float drainRate = 0.35f; // Тратится при ускорении
    public float speedMultiplier = 1.6f;

    private CarController car;
    private float currentCharge = 0f;
    private bool isBoosting = false;

    private void Awake()
    {
        car = GetComponent<CarController>();
    }

    private void Update()
    {
        if (isBoosting)
        {
            currentCharge -= drainRate * Time.deltaTime;
            if (currentCharge <= 0f)
            {
                currentCharge = 0f;
                StopBoost();
            }
        }
        else
        {
            // Со временем копится
            currentCharge = Mathf.Min(currentCharge + chargeRate * Time.deltaTime, maxCharge);
        }
    }

    public void TryActivateNitro()
    {
        if (isBoosting || currentCharge <= 0f || car == null) return;
        isBoosting = true;
        car.SetNitroMultiplier(speedMultiplier);
        if (UIManager.Instance != null) UIManager.Instance.ShowNitroActive(true);
    }

    private void StopBoost()
    {
        isBoosting = false;
        if (car != null) car.SetNitroMultiplier(1f);
        if (UIManager.Instance != null) UIManager.Instance.ShowNitroActive(false);
    }

    // ЭТОТ МЕТОД ВЫЗЫВАЕТСЯ ПРИ ПОДБОРЕ БАНКИ НА КАРТЕ
    public void AddCharge(float amount)
    {
        currentCharge = Mathf.Clamp(currentCharge + amount, 0f, maxCharge);
    }

    public float GetChargeNormalized() => currentCharge / maxCharge;
    public bool IsBoosting() => isBoosting;
    public bool IsFull() => currentCharge >= maxCharge;
}