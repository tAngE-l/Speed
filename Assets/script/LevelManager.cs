using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("Настройки текущего уровня")]
    public int requiredCheckpoints = 5; // Сколько КП нужно для победы
    public float timeLimit = 90f;       // Лимит времени
    
    [Header("Условия для звезд")]
    public float timeFor3Stars = 30f;   // Остаток времени для 3 звезд
    public float timeFor2Stars = 15f;   // Остаток времени для 2 звезд

    [Header("Ссылки на системы")]
    public CheckpointManager checkpointManager;
    public RaceTimer raceTimer;
    public ResultsScreen resultsScreen;

    private bool isLevelOver = false;

    private void Start()
    {
        // Применяем настройки из инспектора к системам
        if (raceTimer != null) raceTimer.startTime = timeLimit;
        if (checkpointManager != null) checkpointManager.requiredCount = requiredCheckpoints;

        // Подписываемся на события
        if (checkpointManager != null) checkpointManager.onLevelComplete.AddListener(OnLevelWin);
        if (checkpointManager != null) checkpointManager.onLevelFailed.AddListener(OnLevelLose);
        if (raceTimer != null) raceTimer.onTimeOut.AddListener(OnLevelLose);
    }

    private void OnLevelWin()
    {
        if (isLevelOver) return;
        isLevelOver = true;

        // Считаем звезды
        float timeLeft = raceTimer.GetTimeRemaining();
        int stars = 1;
        if (timeLeft >= timeFor2Stars) stars = 2;
        if (timeLeft >= timeFor3Stars) stars = 3;

        SaveStars(stars);
        resultsScreen.ShowResults(true, stars);
    }

    private void OnLevelLose()
    {
        if (isLevelOver) return;
        isLevelOver = true;
        resultsScreen.ShowResults(false, 0);
    }

    private void SaveStars(int stars)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        int savedStars = PlayerPrefs.GetInt("Stars_" + sceneName, 0);
        
        if (stars > savedStars)
        {
            PlayerPrefs.SetInt("Stars_" + sceneName, stars);
            PlayerPrefs.Save();
        }
    }
}