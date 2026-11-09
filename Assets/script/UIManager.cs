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

    [Header("Управление")]
    public Button btnLeft;       
    public Button btnRight;      
    public VirtualSteeringWheel steeringWheel;
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

    [Header("НАСТРОЙКИ (UI)")]
    public GameObject settingsMenu;     // Сама панель настроек
    public Button btnOpenSettings;      // Кнопка открытия
    public Button btnCloseSettings;     // Кнопка закрытия (крестик)
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider sensitivitySlider;
    public TMP_Dropdown qualityDropdown;
    public Button btnSaveSettings;

    [Header("Выбор уровней")]
    public GameObject levelSelectMenu; 
    public Button btnOpenLevels;       
    public Button btnCloseLevels;      
    public Button btnLevel1;           
    public Button btnLevel2;           
    public Button btnLevel3;           
    public Button btnPlay;             

    private NitroSystem nitroSystem;
    private CarController car;
    private bool isPaused = false;
    private int selectedLevelIndex = 1;

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

        // Кнопки
        if (btnGas != null) holdGas = btnGas.gameObject.AddComponent<ButtonHold>();
        if (btnBrake != null) holdBrake = btnBrake.gameObject.AddComponent<ButtonHold>();
        if (btnLeft != null) holdLeft = btnLeft.gameObject.AddComponent<ButtonHold>();
        if (btnRight != null) holdRight = btnRight.gameObject.AddComponent<ButtonHold>();
        if (btnNitro != null) btnNitro.onClick.AddListener(() => car?.ActivateNitro());

        // Меню паузы
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (pauseButton != null) pauseButton.onClick.AddListener(TogglePause);
        if (btnResume != null) btnResume.onClick.AddListener(TogglePause);
        if (btnRestart != null) btnRestart.onClick.AddListener(RestartLevel);
        if (btnQuit != null) btnQuit.onClick.AddListener(QuitGame);
        if (btnToggleControl != null) btnToggleControl.onClick.AddListener(ToggleControlMode);
        
        // Настройки
        if (settingsMenu != null) settingsMenu.SetActive(false);
        if (btnOpenSettings != null) btnOpenSettings.onClick.AddListener(OpenSettings);
        if (btnCloseSettings != null) btnCloseSettings.onClick.AddListener(CloseSettings);
        if (btnSaveSettings != null) btnSaveSettings.onClick.AddListener(SaveSettings);

        // Выбор уровней
        if (levelSelectMenu != null) levelSelectMenu.SetActive(false);
        if (btnOpenLevels != null) btnOpenLevels.onClick.AddListener(OpenLevelMenu);
        if (btnCloseLevels != null) btnCloseLevels.onClick.AddListener(() => levelSelectMenu.SetActive(false));
        if (btnLevel1 != null) btnLevel1.onClick.AddListener(() => SelectLevel(1));
        if (btnLevel2 != null) btnLevel2.onClick.AddListener(() => SelectLevel(2));
        if (btnLevel3 != null) btnLevel3.onClick.AddListener(() => SelectLevel(3));
        if (btnPlay != null) btnPlay.onClick.AddListener(PlaySelectedLevel);

        ApplyControlMode(); 
        LoadSettings();
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

    // --- ЛОГИКА НАСТРОЕК ---
    private void OpenSettings()
    {
        if (settingsMenu != null) settingsMenu.SetActive(true);
    }

    private void CloseSettings()
    {
        if (settingsMenu != null) settingsMenu.SetActive(false);
    }

    private void SaveSettings()
    {
        if (musicSlider != null) PlayerPrefs.SetFloat("VolumeMusic", musicSlider.value);
        if (sfxSlider != null) PlayerPrefs.SetFloat("VolumeSFX", sfxSlider.value);
        if (sensitivitySlider != null) PlayerPrefs.SetFloat("SteerSensitivity", sensitivitySlider.value);
        if (qualityDropdown != null) PlayerPrefs.SetInt("GraphicsQuality", qualityDropdown.value);
        PlayerPrefs.Save();
        ApplySettings();
        Debug.Log("Настройки сохранены!");
    }

    private void LoadSettings()
    {
        float musicVol = PlayerPrefs.GetFloat("VolumeMusic", 0.8f);
        float sfxVol = PlayerPrefs.GetFloat("VolumeSFX", 0.8f);
        float sensitivity = PlayerPrefs.GetFloat("SteerSensitivity", 1.0f);
        int quality = PlayerPrefs.GetInt("GraphicsQuality", 1);

        if (musicSlider != null) musicSlider.value = musicVol;
        if (sfxSlider != null) sfxSlider.value = sfxVol;
        if (sensitivitySlider != null) sensitivitySlider.value = sensitivity;
        if (qualityDropdown != null) qualityDropdown.value = quality;

        ApplySettings();
    }

    private void ApplySettings()
    {
        if (musicSlider != null) AudioListener.volume = musicSlider.value;
        if (sensitivitySlider != null && steeringWheel != null) 
            steeringWheel.tiltSensitivity = sensitivitySlider.value;
        if (qualityDropdown != null) QualitySettings.SetQualityLevel(qualityDropdown.value);
    }

    // --- ЛОГИКА УРОВНЕЙ ---
    private void OpenLevelMenu()
    {
        if (levelSelectMenu != null) levelSelectMenu.SetActive(true);
    }

    private void SelectLevel(int levelNumber)
    {
        selectedLevelIndex = levelNumber;
        PlayerPrefs.SetInt("SelectedLevel", levelNumber);
    }

    private void PlaySelectedLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(selectedLevelIndex);
    }

    // --- ОСТАЛЬНОЕ ---
    public void ShowNitroActive(bool active) { if (nitroActiveIndicator != null) nitroActiveIndicator.SetActive(active); }

    private void ToggleControlMode()
    {
        if (car != null) { car.useWheel = !car.useWheel; ApplyControlMode(); }
    }

    private void ApplyControlMode()
    {
        if (car == null) return;
        if (btnLeftObj != null) btnLeftObj.SetActive(!car.useWheel);
        if (btnRightObj != null) btnRightObj.SetActive(!car.useWheel);
        if (wheelObj != null) wheelObj.SetActive(car.useWheel);
        if (controlText != null) controlText.text = car.useWheel ? "СМЕНИТЬ НА КНОПКИ" : "СМЕНИТЬ НА РУЛЬ";
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        if (isPaused) { pauseMenu.SetActive(true); Time.timeScale = 0f; }
        else { pauseMenu.SetActive(false); Time.timeScale = 1f; }
    }

    private void RestartLevel() { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
    private void QuitGame() { Time.timeScale = 1f; Application.Quit(); }
}