using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Тексты и Шкалы")]
    public TextMeshProUGUI checkpointText;
    public Image nitroBarFill;
    public GameObject nitroReadyIndicator;
    public GameObject nitroActiveIndicator;
    public TextMeshProUGUI speedText; 

    [Header("Кнопки газа/тормоза")]
    public Button btnGas;
    public Button btnBrake;
    public Button btnNitro;

    [Header("Управление (Кнопки или Руль)")]
    public Button btnLeft;       
    public Button btnRight;      
    public VirtualSteeringWheel steeringWheel; // Ссылка на наш новый скрипт
    public GameObject btnLeftObj;  
    public GameObject btnRightObj; 
    public GameObject wheelObj;    

    [Header("Меню паузы")]
    public GameObject pauseMenu;
    public Button pauseButton;    
    public Button btnResume;      
    public Button btnRestart;     
    public Button btnQuit;        
    public Button btnToggleControl; 
    public TextMeshProUGUI controlText; 

    private NitroSystem nitroSystem;
    private CarController car;
    private bool isPaused = false;

    private class ButtonHold : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public bool isPressed = false;
        public void OnPointerDown(PointerEventData eventData) => isPressed = true;
        public void OnPointerUp(PointerEventData eventData) => isPressed = false;
    }

    private ButtonHold holdGas, holdBrake, holdLeft, holdRight;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        car = FindObjectOfType<CarController>();
        if (car != null) nitroSystem = car.GetComponent<NitroSystem>();

        if (CheckpointManager.Instance != null)
            CheckpointManager.Instance.onCheckpointCollected.AddListener((c, r) => checkpointText.text = $"КП: {c} / {r}");

        if (nitroReadyIndicator != null) nitroReadyIndicator.SetActive(false);
        if (nitroActiveIndicator != null) nitroActiveIndicator.SetActive(false);

        // Настройка кнопок
        if (btnGas != null) holdGas = btnGas.gameObject.AddComponent<ButtonHold>();
        if (btnBrake != null) holdBrake = btnBrake.gameObject.AddComponent<ButtonHold>();
        if (btnLeft != null) holdLeft = btnLeft.gameObject.AddComponent<ButtonHold>();
        if (btnRight != null) holdRight = btnRight.gameObject.AddComponent<ButtonHold>();
        if (btnNitro != null) btnNitro.onClick.AddListener(() => car?.ActivateNitro());

        // Меню
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (pauseButton != null) pauseButton.onClick.AddListener(TogglePause);
        if (btnResume != null) btnResume.onClick.AddListener(TogglePause);
        if (btnRestart != null) btnRestart.onClick.AddListener(RestartLevel);
        if (btnQuit != null) btnQuit.onClick.AddListener(QuitGame);

        // Кнопка смены управления
        if (btnToggleControl != null) btnToggleControl.onClick.AddListener(ToggleControlMode);

        // Применяем начальный вид управления
        ApplyControlMode(); 
    }

    private void Update()
    {
        if (car == null || isPaused) return;

        car.SetGas(holdGas != null && holdGas.isPressed);
        car.SetBrake(holdBrake != null && holdBrake.isPressed);
        
        if (!car.useWheel)
        {
            car.SetLeft(holdLeft != null && holdLeft.isPressed);
            car.SetRight(holdRight != null && holdRight.isPressed);
        }
        else if (steeringWheel != null)
        {
            // БЕРЕМ ЗНАЧЕНИЕ ОТ РУЛЯ
            car.SetSteer(steeringWheel.GetSteeringValue());
        }

        if (nitroSystem != null && nitroBarFill != null)
            nitroBarFill.fillAmount = nitroSystem.GetChargeNormalized();
            
        if (nitroSystem != null && nitroReadyIndicator != null)
            nitroReadyIndicator.SetActive(nitroSystem.IsFull() && !nitroSystem.IsBoosting());

        if (speedText != null)
        {
            int currentSpeed = Mathf.RoundToInt(car.GetSpeedKmh());
            speedText.text = $"{currentSpeed} <size=50%>км/ч</size>";
        }
    }

    public void ShowNitroActive(bool active)
    {
        if (nitroActiveIndicator != null) nitroActiveIndicator.SetActive(active);
    }

    private void ToggleControlMode()
    {
        if (car != null)
        {
            car.useWheel = !car.useWheel; 
            ApplyControlMode(); 
        }
    }

    private void ApplyControlMode()
    {
        if (car == null) return;

        if (btnLeftObj != null) btnLeftObj.SetActive(!car.useWheel);
        if (btnRightObj != null) btnRightObj.SetActive(!car.useWheel);
        if (wheelObj != null) wheelObj.SetActive(car.useWheel);

        if (controlText != null)
        {
            controlText.text = car.useWheel ? "СМЕНИТЬ НА КНОПКИ" : "СМЕНИТЬ НА РУЛЬ";
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        if (isPaused) { pauseMenu.SetActive(true); Time.timeScale = 0f; }
        else { pauseMenu.SetActive(false); Time.timeScale = 1f; }
    }

    private void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}