using UnityEngine;
using UnityEngine.Events;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    public int requiredCount = 5;
    public Checkpoint[] allCheckpoints;
    public RaceTimer raceTimer;
    
    public UnityEvent onLevelComplete;
    public UnityEvent onLevelFailed;
    public UnityEvent<int, int> onCheckpointCollected;

    private int nextExpectedIndex = 0;
    private int collectedCount = 0;
    private bool raceOver = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        foreach (var cp in allCheckpoints) cp.ResetCheckpoint();
        if (raceTimer != null) raceTimer.onTimeOut.AddListener(OnTimeOut);
        UpdateUI();
    }

    public void TryCollectCheckpoint(Checkpoint cp)
    {
        if (raceOver || cp.orderIndex != nextExpectedIndex) return;

        cp.MarkCollected();
        collectedCount++;
        nextExpectedIndex++;
        UpdateUI();

        if (cp.isFinish && collectedCount >= requiredCount)
        {
            raceOver = true;
            if (raceTimer != null) raceTimer.StopTimer();
            onLevelComplete?.Invoke();
        }
    }

    private void OnTimeOut()
    {
        if (raceOver) return;
        raceOver = true;
        onLevelFailed?.Invoke();
    }

    private void UpdateUI() => onCheckpointCollected?.Invoke(collectedCount, requiredCount);
}