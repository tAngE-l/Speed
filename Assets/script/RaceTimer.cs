using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class RaceTimer : MonoBehaviour
{
    public float startTime = 90f;
    public TextMeshProUGUI timerText;
    public Color normalColor = Color.white;
    public Color warningColor = Color.yellow;
    public Color dangerColor = Color.red;
    
    public UnityEvent onTimeOut;

    private float timeRemaining;
    private bool isRunning = false;

    private void Start()
    {
        timeRemaining = startTime;
        isRunning = true;
        UpdateText();
    }

    private void Update()
    {
        if (!isRunning) return;
        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            isRunning = false;
            UpdateText();
            onTimeOut?.Invoke();
            return;
        }
        UpdateText();
    }

    private void UpdateText()
    {
        if (timerText == null) return;
        int m = Mathf.FloorToInt(timeRemaining / 60f);
        int s = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = $"{m:00}:{s:00}";
        timerText.color = timeRemaining <= 5f ? dangerColor : (timeRemaining <= 15f ? warningColor : normalColor);
    }

    public void StopTimer() => isRunning = false;
}