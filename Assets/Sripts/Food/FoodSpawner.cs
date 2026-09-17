using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Genera comida en posiciones aleatorias dentro de un área rectangular,
/// manteniendo como máximo maxFood unidades activas al mismo tiempo.
/// </summary>
public class FoodSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject foodPrefab;

    [Header("Configuración")]
    public int maxFood = 10;
    public float spawnInterval = 3f;
    public Vector3 areaSize = new Vector3(20f, 0f, 20f);

    private readonly List<GameObject> activeFood = new List<GameObject>();
    private float timer;

    void Start()
    {
        // Spawnea un lote inicial para no arrancar con el escenario vacío.
        for (int i = 0; i < maxFood; i++)
            SpawnFood();
    }

    void Update()
    {
        activeFood.RemoveAll(f => f == null);

        timer += Time.deltaTime;
        if (timer >= spawnInterval && activeFood.Count < maxFood)
        {
            timer = 0f;
            SpawnFood();
        }
    }

    private void SpawnFood()
    {
        if (foodPrefab == null) return;

        Vector3 pos = transform.position + new Vector3(
            Random.Range(-areaSize.x / 2f, areaSize.x / 2f),
            0f,
            Random.Range(-areaSize.z / 2f, areaSize.z / 2f));

        if (WorldBounds.Instance != null)
        {
            pos.y = WorldBounds.Instance.FixedHeight;
            Renderer rend = foodPrefab.GetComponentInChildren<Renderer>();
            if (rend != null)
                pos.y += rend.bounds.extents.y;
        }

        GameObject food = Instantiate(foodPrefab, pos, Quaternion.identity);
        activeFood.Add(food);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawCube(transform.position, areaSize);
    }
}
