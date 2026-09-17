using UnityEngine;

/// <summary>
/// Comida individual. El propio Boid llama a Consume() cuando llega y la come
/// (no usamos triggers/Rigidbody para esto, se resuelve por distancia).
/// </summary>
public class Food : MonoBehaviour
{
    private bool consumed;

    public void Consume()
    {
        if (consumed) return;
        consumed = true;
        Destroy(gameObject);
    }
}
