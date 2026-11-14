using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Header("Движение")]
    public float maxSpeed = 30f;
    public float acceleration = 20f; 
    public float turnSpeed = 150f;
    public float brakeForce = 40f;
    private float reverseSpeedLimit = 16.6f; 

    [Header("Управление")]
    public bool useWheel = false;

    // Текущие параметры (меняются в зависимости от тега дороги)
    private float currentGrip = 1.0f; 
    private float currentSpeedMult = 1.0f;
    private string currentSurfaceName = "Асфальт";

    private Rigidbody rb;
    private NitroSystem nitroSystem;

    private bool gasPressed = false;
    private bool brakePressed = false;
    private bool leftPressed = false;
    private bool rightPressed = false;
    private float joystickSteer = 0f;
    private float nitroMultiplier = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        nitroSystem = GetComponent<NitroSystem>();
    }

    private void Update()
    {
        // Клавиатура (для ПК)
        if (Input.GetKey(KeyCode.W)) gasPressed = true; else if (Input.GetKeyUp(KeyCode.W)) gasPressed = false;
        if (Input.GetKey(KeyCode.S)) brakePressed = true; else if (Input.GetKeyUp(KeyCode.S)) brakePressed = false;

        if (!useWheel)
        {
            if (Input.GetKey(KeyCode.A)) leftPressed = true; else if (Input.GetKeyUp(KeyCode.A)) leftPressed = false;
            if (Input.GetKey(KeyCode.D)) rightPressed = true; else if (Input.GetKeyUp(KeyCode.D)) rightPressed = false;
        }

        if (Input.GetKeyDown(KeyCode.Space) && nitroSystem != null) nitroSystem.TryActivateNitro();
    }

    public void SetGas(bool state) => gasPressed = state;
    public void SetBrake(bool state) => brakePressed = state;
    public void SetLeft(bool state) => leftPressed = state;
    public void SetRight(bool state) => rightPressed = state;
    public void SetSteer(float steerValue) => joystickSteer = steerValue;
    public void ActivateNitro() => nitroSystem?.TryActivateNitro();
    public void SetNitroMultiplier(float mult) => nitroMultiplier = mult;

    public float GetSpeedKmh()
    {
        float forwardSpeed = transform.InverseTransformDirection(rb.linearVelocity).z;
        return Mathf.Abs(forwardSpeed) * 3.6f; 
    }

    public string GetSurfaceName() => currentSurfaceName;

    private void FixedUpdate()
    {
        // === 1. ОПРЕДЕЛЯЕМ ПОВЕРХНОСТЬ (по тегу) ===
        RaycastHit hit;
        // Пускаем луч из центра машины вниз (на 2 метра)
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
        {
            switch (hit.collider.tag)
            {
                case "Road_Gravel": 
                    currentGrip = 0.78f; 
                    currentSpeedMult = 1.0f; 
                    currentSurfaceName = "Гравий"; 
                    break;
                case "Road_Sand": 
                    currentGrip = 0.72f; 
                    currentSpeedMult = 0.82f; 
                    currentSurfaceName = "Песок"; 
                    break;
                case "Road_Dirt": 
                    currentGrip = 0.62f; 
                    currentSpeedMult = 0.75f; 
                    currentSurfaceName = "Грязь"; 
                    break;
                case "Road_Wet": 
                    currentGrip = 0.70f; 
                    currentSpeedMult = 1.0f; 
                    currentSurfaceName = "Мокрый асфальт"; 
                    break;
                default: // Road_Asphalt или любой другой тег
                    currentGrip = 1.0f; 
                    currentSpeedMult = 1.0f; 
                    currentSurfaceName = "Асфальт"; 
                    break;
            }
        }
        else
        {
            // Если машина в воздухе
            currentGrip = 1.0f;
            currentSpeedMult = 1.0f;
            currentSurfaceName = "В воздухе";
        }

        // === 2. ФИЗИКА ===
        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        float forwardSpeed = localVelocity.z;
        
        // Ограничение скорости от покрытия
        float currentMaxSpeed = maxSpeed * nitroMultiplier * currentSpeedMult;

        if (gasPressed)
        {
            forwardSpeed += acceleration * Time.fixedDeltaTime;
            if (forwardSpeed > currentMaxSpeed) forwardSpeed = currentMaxSpeed;
        }
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

        // === 3. РУЛЬ (Сцепление) ===
        if (Mathf.Abs(forwardSpeed) > 0.5f)
        {
            float turnDir = 0f;
            if (useWheel) turnDir = joystickSteer;
            else
            {
                if (leftPressed) turnDir -= 1f;
                if (rightPressed) turnDir += 1f;
            }

            if (forwardSpeed < 0) turnDir *= -1f;

            float speedFactor = Mathf.Clamp01(Mathf.Abs(forwardSpeed) / 5f);
            
            // Умножаем на currentGrip (на грязи машина будет поворачивать хуже)
            float turn = turnDir * turnSpeed * currentGrip * speedFactor * Time.fixedDeltaTime;
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turn, 0f));
        }
    }
}