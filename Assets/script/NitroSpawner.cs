using System.Collections.Generic;
using UnityEngine;

public class NitroSpawner : MonoBehaviour
{
    public GameObject nitroPrefab;
    public Transform[] spawnPoints;
    public float spawnInterval = 8f;
    public int maxActivePickups = 3;

    private List<GameObject> activePickups = new List<GameObject>();
    private float timer;

    private void Start()
    {
        timer = spawnInterval;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            TrySpawn();
            timer = spawnInterval;
        }
        activePickups.RemoveAll(p => p == null);
    }

    private void TrySpawn()
    {
        if (activePickups.Count >= maxActivePickups || spawnPoints.Length == 0) return;

        int idx = Random.Range(0, spawnPoints.Length);
        GameObject pickup = Instantiate(nitroPrefab, spawnPoints[idx].position, Quaternion.identity);
        activePickups.Add(pickup);
    }
}