using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Header("Движение")]
    public float maxSpeed = 30f;
    public float acceleration = 20f; 
    public float turnSpeed = 150f;
    public float brakeForce = 40f;
    private float reverseSpeedLimit = 16.6f; // 60 км/ч

    [Header("Управление")]
    public bool useWheel = false; // ВЫКЛЮЧЕНО = кнопки, ВКЛЮЧЕНО = руль

    private Rigidbody rb;
    private NitroSystem nitroSystem;

    // Ввод
    private bool gasPressed = false;
    private bool brakePressed = false;
    private bool leftPressed = false;
    private bool rightPressed = false;
    private float joystickSteer = 0f; // Значение от руля
    
    private float nitroMultiplier = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        nitroSystem = GetComponent<NitroSystem>();
    }

    private void Update()
    {
        // Клавиатура (для тестов на ПК)
        if (Input.GetKey(KeyCode.W)) gasPressed = true; else if (Input.GetKeyUp(KeyCode.W)) gasPressed = false;
        if (Input.GetKey(KeyCode.S)) brakePressed = true; else if (Input.GetKeyUp(KeyCode.S)) brakePressed = false;

        // Если руль НЕ выбран, читаем A и D с клавиатуры
        if (!useWheel)
        {
            if (Input.GetKey(KeyCode.A)) leftPressed = true; else if (Input.GetKeyUp(KeyCode.A)) leftPressed = false;
            if (Input.GetKey(KeyCode.D)) rightPressed = true; else if (Input.GetKeyUp(KeyCode.D)) rightPressed = false;
        }

        if (Input.GetKeyDown(KeyCode.Space) && nitroSystem != null)
            nitroSystem.TryActivateNitro();
    }

    // Для кнопок
    public void SetGas(bool state) => gasPressed = state;
    public void SetBrake(bool state) => brakePressed = state;
    public void SetLeft(bool state) => leftPressed = state;
    public void SetRight(bool state) => rightPressed = state;
    
    // Для руля
    public void SetSteer(float steerValue) => joystickSteer = steerValue;
    
    public void ActivateNitro() => nitroSystem?.TryActivateNitro();
    public void SetNitroMultiplier(float mult) => nitroMultiplier = mult;

    public float GetSpeedKmh()
    {
        float forwardSpeed = transform.InverseTransformDirection(rb.linearVelocity).z;
        return Mathf.Abs(forwardSpeed) * 3.6f; 
    }

    private void FixedUpdate()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        float forwardSpeed = localVelocity.z;
        float currentMaxSpeed = maxSpeed * nitroMultiplier;

        // ГАЗ
        if (gasPressed)
        {
            forwardSpeed += acceleration * Time.fixedDeltaTime;
            if (forwardSpeed > currentMaxSpeed) forwardSpeed = currentMaxSpeed;
        }
        // ТОРМОЗ / ЗАДНИЙ ХОД
        else if (brakePressed)
        {
            if (forwardSpeed > 0.1f) forwardSpeed -= brakeForce * Time.fixedDeltaTime;
            else
            {
                forwardSpeed -= acceleration * Time.fixedDeltaTime;
                if (forwardSpeed < -reverseSpeedLimit) forwardSpeed = -reverseSpeedLimit;
            }
        }
        else forwardSpeed = Mathf.MoveTowards(forwardSpeed, 0, 5f * Time.fixedDeltaTime);

        localVelocity.z = forwardSpeed;
        rb.linearVelocity = transform.TransformDirection(localVelocity);

        // РУЛЬ
        if (Mathf.Abs(forwardSpeed) > 0.5f)
        {
            float turnDir = 0f;

            if (useWheel)
            {
                turnDir = joystickSteer; // Берем значение от джойстика
            }
            else
            {
                if (leftPressed) turnDir -= 1f; // Берем значение от кнопок
                if (rightPressed) turnDir += 1f;
            }

            // Задним ходом руль инвертируется
            if (forwardSpeed < 0) turnDir *= -1f;

            float speedFactor = Mathf.Clamp01(Mathf.Abs(forwardSpeed) / 5f);
            float turn = turnDir * turnSpeed * speedFactor * Time.fixedDeltaTime;
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turn, 0f));
        }
    }
}