using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ResultsScreen : MonoBehaviour
{
    [Header("UI Элементы")]
    public GameObject panel;
    public TextMeshProUGUI titleText;

    [Header("Кнопки")]
    public Button btnRestart;
    public Button btnContinue; 
    public Button btnMainMenu;

    [Header("Настройки сюжета")]
    public string nextLevelName; 

    // Скрытые переменные, которые найдутся сами
    private Image[] stars = new Image[3];

    private void Start()
    {
        if (panel != null) panel.SetActive(false);
        
        // Автоматически находим кнопки, если они не привязаны
        if (btnRestart == null) btnRestart = transform.Find("BtnRestart")?.GetComponent<Button>();
        if (btnContinue == null) btnContinue = transform.Find("BtnNext")?.GetComponent<Button>();
        if (btnMainMenu == null) btnMainMenu = transform.Find("BtnMenu")?.GetComponent<Button>();

        // Привязываем кнопки
        if (btnRestart != null) btnRestart.onClick.AddListener(RestartLevel);
        if (btnContinue != null) btnContinue.onClick.AddListener(ContinueToNextLevel);
        if (btnMainMenu != null) btnMainMenu.onClick.AddListener(GoToMainMenu);

        // АВТОМАТИЧЕСКИЙ ПОИСК ЗВЕЗД (по именам в Hierarchy)
        stars[0] = transform.Find("Star")?.GetComponent<Image>();
        stars[1] = transform.Find("Star (1)")?.GetComponent<Image>();
        stars[2] = transform.Find("Star (2)")?.GetComponent<Image>();
    }

    public void ShowResults(bool isWin, int starCount)
    {
        if (panel != null) panel.SetActive(true);
        Time.timeScale = 0f;

        if (titleText != null)
        {
            titleText.text = isWin ? "ПОБЕДА!" : "ВРЕМЯ ВЫШЛО";
            titleText.color = isWin ? Color.green : Color.red;
        }

        // Красим звезды
        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] != null)
            {
                stars[i].gameObject.SetActive(true);
                if (i < starCount) 
                    stars[i].color = Color.yellow; // Желтый
                else 
                    stars[i].color = new Color(0.3f, 0.3f, 0.3f); // Темно-серый
            }
            else
            {
                Debug.LogError($"Звезда {i} не найдена! Проверь имена объектов в Hierarchy (должно быть Star, Star (1), Star (2))");
            }
        }

        if (btnContinue != null) btnContinue.gameObject.SetActive(isWin);
    }

    private void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ContinueToNextLevel()
    {
        Time.timeScale = 1f;
        if (UIManager.Instance != null) UIManager.Instance.LevelCompleted(nextLevelName);
        SceneManager.LoadScene(nextLevelName); 
    }

    private void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}