using UnityEngine;

/// <summary>
/// Área rectangular (en el plano XZ) dentro de la cual deben mantenerse
/// los boids y el cazador. Se pone directamente sobre el GameObject del
/// piso: por defecto, detecta el tamaño automáticamente a partir de su
/// Renderer o Collider (Mesh Renderer del Plane, Box Collider, etc.), así
/// que no hay que tipear ningún número a mano.
/// </summary>
[DisallowMultipleComponent]
public class WorldBounds : MonoBehaviour
{
    public static WorldBounds Instance { get; private set; }

    [Header("Detección automática")]
    [Tooltip("Si está activo, calcula el área a partir del Renderer/Collider de ESTE objeto (el piso). Desactivalo para escribir los valores a mano.")]
    public bool autoDetectFromFloor = true;

    [Header("Área manual (solo si Auto Detect está desactivado)")]
    public Vector3 manualCenter = Vector3.zero;
    public Vector2 manualSize = new Vector2(20f, 20f); // x = ancho (eje X), y = profundidad (eje Z)

    [Header("Comportamiento cerca del borde")]
    [Tooltip("Distancia al borde a partir de la cual el agente empieza a girar hacia el centro.")]
    public float edgeMargin = 3f;

    public Vector3 Center { get; private set; }
    public Vector2 Size { get; private set; }
    public float FixedHeight { get; private set; }

    void Awake()
    {
        Instance = this;
        CalculateBounds();
    }

    private void CalculateBounds()
    {
        if (autoDetectFromFloor)
        {
            Bounds b;
            Renderer rend = GetComponent<Renderer>();
            Collider col = GetComponent<Collider>();

            if (rend != null) b = rend.bounds;
            else if (col != null) b = col.bounds;
            else
            {
                Debug.LogWarning("WorldBounds: este objeto no tiene Renderer ni Collider, usando el área manual.");
                Center = manualCenter;
                Size = manualSize;
                FixedHeight = manualCenter.y;
                return;
            }

            Center = b.center;
            Size = new Vector2(b.size.x, b.size.z);
            FixedHeight = b.max.y; // apoya a los agentes sobre la superficie del piso
        }
        else
        {
            Center = manualCenter;
            Size = manualSize;
            FixedHeight = manualCenter.y;
        }
    }

    public float MinX => Center.x - Size.x / 2f;
    public float MaxX => Center.x + Size.x / 2f;
    public float MinZ => Center.z - Size.y / 2f;
    public float MaxZ => Center.z + Size.y / 2f;

    /// <summary>
    /// Steering que empuja suavemente al agente hacia el centro del área
    /// cuando se acerca a un borde. Devuelve Vector3.zero si está lejos de todos los bordes.
    /// </summary>
    public Vector3 Contain(Vector3 position, Vector3 velocity, float maxSpeed, float maxForce)
    {
        bool near = position.x < MinX + edgeMargin || position.x > MaxX - edgeMargin ||
                    position.z < MinZ + edgeMargin || position.z > MaxZ - edgeMargin;

        if (!near) return Vector3.zero;

        Vector3 steerTarget = Center;
        steerTarget.y = position.y;
        return SteeringBehaviors.Seek(position, velocity, steerTarget, maxSpeed, maxForce);
    }

    /// <summary>Red de seguridad: garantiza que la posición final quede dentro del área.</summary>
    /// <param name="heightOffset">
    /// Cuánto levantar al agente por encima de la superficie del piso.
    /// Si el pivote del modelo está en su centro (no en la base), usá la
    /// mitad de su altura para que no quede hundido en el piso.
    /// </param>
    public Vector3 ClampToArea(Vector3 position, float heightOffset = 0f)
    {
        position.x = Mathf.Clamp(position.x, MinX, MaxX);
        position.z = Mathf.Clamp(position.z, MinZ, MaxZ);
        position.y = FixedHeight + heightOffset;
        return position;
    }

    void OnDrawGizmos()
    {
        // Recalcula en el editor (fuera de Play) para que el gizmo siempre muestre el área real.
        if (!Application.isPlaying) CalculateBounds();

        Gizmos.color = Color.green;
        Vector3 c = new Vector3(Center.x, FixedHeight, Center.z);
        Vector3 s = new Vector3(Size.x, 0.01f, Size.y);
        Gizmos.DrawWireCube(c, s);
    }
}
