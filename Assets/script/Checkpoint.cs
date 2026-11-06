using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int orderIndex;
    public bool isFinish;
    private bool collected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        // Проверяем, что в нас врезалась именно машина (есть CarController)
        if (other.GetComponent<CarController>() != null)
        {
            CheckpointManager.Instance.TryCollectCheckpoint(this);
        }
    }

    public void MarkCollected()
    {
        collected = true;
        gameObject.SetActive(false); // Выключаем объект
    }

    public void ResetCheckpoint()
    {
        collected = false;
        gameObject.SetActive(true);
    }
}