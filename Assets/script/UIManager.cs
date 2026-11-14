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

    [Header("НАСТРОЙКИ")]
    public GameObject settingsMenu;     
    public Button btnOpenSettings;      
    public Button btnCloseSettings;     
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider sensitivitySlider;
    public TMP_Dropdown qualityDropdown;
    public Button btnSaveSettings;

    [Header("Выбор уровней (Панель)")]
    public GameObject levelSelectMenu; 
    public Button btnCloseLevels;      
    public Button btnLevel1;           
    public Button btnLevel2;           
    public Button btnLevel3;           
    
    [Header("ЗВЕЗДЫ В МЕНЮ ВЫБОРА")]
    public Image[] level1Stars; 
    public Image[] level2Stars; 
    public Image[] level3Stars; 
    
    [Header("ГЛАВНОЕ МЕНЮ")]
    public GameObject mainMenuPanel; 
    public Button btnPlayGame;       
    public Button btnOpenLevels;     
    public Button btnMainQuit;       
    public Button btnOpenGarage;     

    [Header("ГАРАЖ")]
    public Button btnGarageBack;     

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
        string currentScene = SceneManager.GetActiveScene().name;

        // ==========================================
        // НАСТРОЙКА ДЛЯ ГЛАВНОГО МЕНЮ
        // ==========================================
        if (currentScene == "MainMenu")
        {
            if (levelSelectMenu != null) levelSelectMenu.SetActive(false);
            if (settingsMenu != null) settingsMenu.SetActive(false);
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);

            if (btnPlayGame != null) btnPlayGame.onClick.AddListener(PlayStoryMode);
            if (btnOpenLevels != null) btnOpenLevels.onClick.AddListener(OpenLevelMenu);
            if (btnCloseLevels != null) btnCloseLevels.onClick.AddListener(() => { if (levelSelectMenu != null) levelSelectMenu.SetActive(false); });
            
            if (btnLevel1 != null) btnLevel1.onClick.AddListener(() => LoadLevelDirectly("Level_1"));
            if (btnLevel2 != null) btnLevel2.onClick.AddListener(() => LoadLevelDirectly("Level_2"));
            if (btnLevel3 != null) btnLevel3.onClick.AddListener(() => LoadLevelDirectly("Level_3"));

            if (btnOpenSettings != null) btnOpenSettings.onClick.AddListener(OpenSettings);
            if (btnCloseSettings != null) btnCloseSettings.onClick.AddListener(CloseSettings);
            if (btnSaveSettings != null) btnSaveSettings.onClick.AddListener(SaveSettings);
            if (btnMainQuit != null) btnMainQuit.onClick.AddListener(QuitGame);
            if (btnOpenGarage != null) btnOpenGarage.onClick.AddListener(GoToGarage);

            LoadSettings();
            return; 
        }

        // ==========================================
        // НАСТРОЙКА ДЛЯ ГАРАЖА
        // ==========================================
        if (currentScene == "Garage")
        {
            if (btnGarageBack != null) btnGarageBack.onClick.AddListener(GoToMainMenu);
            LoadSettings();
            return;
        }

        // ==========================================
        // НАСТРОЙКА ДЛЯ УРОВНЕЙ
        // ==========================================
        car = FindObjectOfType<CarController>();
        if (car != null) nitroSystem = car.GetComponent<NitroSystem>();

        if (CheckpointManager.Instance != null)
            CheckpointManager.Instance.onCheckpointCollected.AddListener((c, r) => checkpointText.text = $"КП: {c} / {r}");

        if (nitroReadyIndicator != null) nitroReadyIndicator.SetActive(false);
        if (nitroActiveIndicator != null) nitroActiveIndicator.SetActive(false);

        if (btnGas != null) holdGas = btnGas.gameObject.AddComponent<ButtonHold>();
        if (btnBrake != null) holdBrake = btnBrake.gameObject.AddComponent<ButtonHold>();
        if (btnLeft != null) holdLeft = btnLeft.gameObject.AddComponent<ButtonHold>();
        if (btnRight != null) holdRight = btnRight.gameObject.AddComponent<ButtonHold>();
        if (btnNitro != null) btnNitro.onClick.AddListener(() => car?.ActivateNitro());

        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (pauseButton != null) pauseButton.onClick.AddListener(TogglePause);
        if (btnResume != null) btnResume.onClick.AddListener(TogglePause);
        if (btnRestart != null) btnRestart.onClick.AddListener(RestartLevel);
        if (btnQuit != null) btnQuit.onClick.AddListener(GoToMainMenu);
        if (btnOpenGarage != null) btnOpenGarage.onClick.AddListener(GoToGarageFromPause);
        if (btnToggleControl != null) btnToggleControl.onClick.AddListener(ToggleControlMode);
        
        if (settingsMenu != null) settingsMenu.SetActive(false);
        if (btnOpenSettings != null) btnOpenSettings.onClick.AddListener(OpenSettings);
        if (btnCloseSettings != null) btnCloseSettings.onClick.AddListener(CloseSettings);
        if (btnSaveSettings != null) btnSaveSettings.onClick.AddListener(SaveSettings);

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
        else if (steeringWheel != null) car.SetSteer(steeringWheel.GetSteeringValue());

        if (nitroSystem != null && nitroBarFill != null) nitroBarFill.fillAmount = nitroSystem.GetChargeNormalized();
        if (nitroSystem != null && nitroReadyIndicator != null) nitroReadyIndicator.SetActive(nitroSystem.IsFull() && !nitroSystem.IsBoosting());

        if (speedText != null)
        {
            int currentSpeed = Mathf.RoundToInt(car.GetSpeedKmh());
            speedText.text = $"{currentSpeed} <size=50%>км/ч</size>";
        }
    }

    // --- МЕНЮ ВЫБОРА И ЗВЕЗДЫ ---
    private void OpenLevelMenu() 
    { 
        if (levelSelectMenu != null) levelSelectMenu.SetActive(true);
        UpdateLevelStars(); // Обновляем звезды при открытии
    }

    private void UpdateLevelStars()
    {
        // Читаем сохранения из памяти
        int stars1 = PlayerPrefs.GetInt("Stars_Level_1", 0);
        int stars2 = PlayerPrefs.GetInt("Stars_Level_2", 0);
        int stars3 = PlayerPrefs.GetInt("Stars_Level_3", 0);

        SetStarsColor(level1Stars, stars1);
        SetStarsColor(level2Stars, stars2);
        SetStarsColor(level3Stars, stars3);
    }

    private void SetStarsColor(Image[] starsArray, int earnedStars)
    {
        if (starsArray == null) return;

        for (int i = 0; i < starsArray.Length; i++)
        {
            if (starsArray[i] != null)
            {
                if (i < earnedStars) 
                    starsArray[i].color = Color.yellow;
                else 
                    starsArray[i].color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            }
        }
    }

    // --- ЛОГИКА СЮЖЕТА ---
    private void PlayStoryMode()
    {
        string nextScene = PlayerPrefs.GetString("NextLevelToPlay", "Tutorial");
        SceneManager.LoadScene(nextScene);
    }

    private void LoadLevelDirectly(string levelName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(levelName);
    }

    public void LevelCompleted(string nextLevelName)
    {
        PlayerPrefs.SetString("NextLevelToPlay", nextLevelName);
        PlayerPrefs.Save();
    }

    // --- ГАРАЖ И ПЕРЕХОДЫ ---
    private void GoToGarage() { SceneManager.LoadScene("Garage"); }
    private void GoToGarageFromPause() { Time.timeScale = 1f; SceneManager.LoadScene("Garage"); }
    public void GoToMainMenu() { Time.timeScale = 1f; SceneManager.LoadScene("MainMenu"); }

    // --- ОСТАЛЬНОЕ ---
    private void RestartLevel() { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
    private void QuitGame() { Application.Quit(); }

    private void ToggleControlMode() { if (car != null) { car.useWheel = !car.useWheel; ApplyControlMode(); } }
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

    public void ShowNitroActive(bool active) { if (nitroActiveIndicator != null) nitroActiveIndicator.SetActive(active); }

    private void OpenSettings() { if (settingsMenu != null) settingsMenu.SetActive(true); }
    private void CloseSettings() { if (settingsMenu != null) settingsMenu.SetActive(false); }
    private void SaveSettings()
    {
        if (musicSlider != null) PlayerPrefs.SetFloat("VolumeMusic", musicSlider.value);
        if (sfxSlider != null) PlayerPrefs.SetFloat("VolumeSFX", sfxSlider.value);
        if (sensitivitySlider != null) PlayerPrefs.SetFloat("SteerSensitivity", sensitivitySlider.value);
        if (qualityDropdown != null) PlayerPrefs.SetInt("GraphicsQuality", qualityDropdown.value);
        PlayerPrefs.Save(); ApplySettings();
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
        if (sensitivitySlider != null && steeringWheel != null) steeringWheel.tiltSensitivity = sensitivitySlider.value;
        if (qualityDropdown != null) QualitySettings.SetQualityLevel(qualityDropdown.value);
    }
}