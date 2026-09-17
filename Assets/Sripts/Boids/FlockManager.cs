using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton simple que mantiene la lista de todos los Boids activos en la escena.
/// Cada Boid se registra/desregistra solo. Sirve para que cada boid pueda
/// consultar a sus vecinos sin tener que buscar en toda la escena con FindObjectsOfType.
/// </summary>
public class FlockManager : MonoBehaviour
{
    public static FlockManager Instance { get; private set; }

    public List<Boid> Boids { get; private set; } = new List<Boid>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Register(Boid boid)
    {
        if (!Boids.Contains(boid))
            Boids.Add(boid);
    }

    public void Unregister(Boid boid)
    {
        Boids.Remove(boid);
    }

    public List<Boid> GetNeighbors(Boid self, float radius)
    {
        List<Boid> neighbors = new List<Boid>();
        foreach (var b in Boids)
        {
            if (b == self) continue;
            if (Vector3.Distance(self.transform.position, b.transform.position) <= radius)
                neighbors.Add(b);
        }
        return neighbors;
    }
}
