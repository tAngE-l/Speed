using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("Настройки текущего уровня")]
    public int requiredCheckpoints = 5; 
    public float timeLimit = 90f;       
    
    [Header("Условия для звезд")]
    public float timeFor3Stars = 30f;   
    public float timeFor2Stars = 15f;   

    [Header("Ссылки на системы")]
    public CheckpointManager checkpointManager;
    public RaceTimer raceTimer;
    public ResultsScreen resultsScreen;

    private bool isLevelOver = false;

    private void Start()
    {
        // Применяем настройки из инспектора
        if (raceTimer != null) raceTimer.startTime = timeLimit;
        if (checkpointManager != null) checkpointManager.requiredCount = requiredCheckpoints;

        // Подписываемся на события
        if (checkpointManager != null) 
        {
            checkpointManager.onLevelComplete.AddListener(OnLevelWin);
            checkpointManager.onLevelFailed.AddListener(OnLevelLose);
        }
        if (raceTimer != null) raceTimer.onTimeOut.AddListener(OnLevelLose);
    }

    private void OnLevelWin()
    {
        if (isLevelOver) return;
        isLevelOver = true;

        // Считаем звезды
        float timeLeft = raceTimer.GetTimeRemaining();
        int stars = 1; // За победу всегда дают минимум 1 звезду
        if (timeLeft >= timeFor2Stars) stars = 2;
        if (timeLeft >= timeFor3Stars) stars = 3;

        // СОХРАНЯЕМ ЗВЕЗДЫ
        SaveStars(stars);
        
        // Показываем экран победы
        if (resultsScreen != null) resultsScreen.ShowResults(true, stars);
    }

    private void OnLevelLose()
    {
        if (isLevelOver) return;
        isLevelOver = true;
        
        // При поражении звезды не сохраняем, показываем 0
        if (resultsScreen != null) resultsScreen.ShowResults(false, 0);
    }

    private void SaveStars(int stars)
    {
        // Берем имя текущей сцены (например, "Level_1")
        string sceneName = SceneManager.GetActiveScene().name;
        string saveKey = "Stars_" + sceneName;
        
        // Смотрим, сколько звезд уже было сохранено (по умолчанию 0)
        int savedStars = PlayerPrefs.GetInt(saveKey, 0);
        
        // Сохраняем, только если новый результат лучше старого
        if (stars > savedStars)
        {
            PlayerPrefs.SetInt(saveKey, stars);
            PlayerPrefs.Save();
            Debug.Log($"Сохранен новый рекорд для {sceneName}: {stars} звезды");
        }
    }
}