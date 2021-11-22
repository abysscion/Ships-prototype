using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Realizes enemies spawning
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;
    public Color gizmosDrawColor = Color.red;
    [Tooltip("Time in seconds between enemies spawning")]
    public float spawnInterval = 3f;
    [Tooltip("Maximum spawned enemies amount")]
    public int enemiesLimit = 4;

    private List<GameObject> _spawnedEnemies = new List<GameObject>();
    private float _lastSpawnTime;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmosDrawColor;

        foreach (var point in spawnPoints)
            Gizmos.DrawSphere(point.position, 1);

        Gizmos.color = Color.clear;
    }

    private void Update()
    {
        TrySpawnNewEnemy();
    }

    private void TrySpawnNewEnemy()
    {
        if (_spawnedEnemies.Count >= enemiesLimit)
            return;
        if ((Time.unscaledTime - _lastSpawnTime) < spawnInterval)
            return;

        var spawnedEnemy = Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Length)],
                                        spawnPoints[Random.Range(0, spawnPoints.Length)].position,
                                        Quaternion.identity);

        _lastSpawnTime = Time.unscaledTime;
        _spawnedEnemies.Add(spawnedEnemy);
        spawnedEnemy.GetComponent<EnemyShipController>().OnDestroyCallback += OnEnemyDeathRemoveFromList;
    }

    private void OnEnemyDeathRemoveFromList(GameObject sender)
    {
        _spawnedEnemies.Remove(sender);
    }
}
