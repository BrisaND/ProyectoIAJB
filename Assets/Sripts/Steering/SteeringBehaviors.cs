using UnityEngine;

/// <summary>
/// Conjunto de Steering Behaviors clásicos (Reynolds).
/// Todas las funciones devuelven una fuerza de dirección (steering force),
/// ya recortada a maxForce, para sumar a la velocidad actual del agente.
/// </summary>
public static class SteeringBehaviors
{
    public static Vector3 Seek(Vector3 position, Vector3 velocity, Vector3 target, float maxSpeed, float maxForce)
    {
        Vector3 desired = (target - position);
        if (desired.sqrMagnitude < 0.0001f) return Vector3.zero;

        desired = desired.normalized * maxSpeed;
        Vector3 steer = desired - velocity;
        return Vector3.ClampMagnitude(steer, maxForce);
    }

    public static Vector3 Flee(Vector3 position, Vector3 velocity, Vector3 target, float maxSpeed, float maxForce)
    {
        Vector3 desired = (position - target);
        if (desired.sqrMagnitude < 0.0001f) return Vector3.zero;

        desired = desired.normalized * maxSpeed;
        Vector3 steer = desired - velocity;
        return Vector3.ClampMagnitude(steer, maxForce);
    }

    /// <summary>
    /// Igual que Seek pero frena suavemente al entrar en slowRadius.
    /// </summary>
    public static Vector3 Arrive(Vector3 position, Vector3 velocity, Vector3 target, float maxSpeed, float maxForce, float slowRadius)
    {
        Vector3 toTarget = target - position;
        float distance = toTarget.magnitude;

        if (distance < 0.0001f) return Vector3.zero;

        float speed = maxSpeed;
        if (distance < slowRadius)
            speed = maxSpeed * (distance / slowRadius);

        Vector3 desired = toTarget.normalized * speed;
        Vector3 steer = desired - velocity;
        return Vector3.ClampMagnitude(steer, maxForce);
    }

    /// <summary>
    /// Persigue prediciendo la posición futura del objetivo según su velocidad actual.
    /// </summary>
    public static Vector3 Pursuit(Vector3 position, Vector3 velocity, Vector3 targetPosition, Vector3 targetVelocity, float maxSpeed, float maxForce)
    {
        float distance = Vector3.Distance(position, targetPosition);
        float t = maxSpeed > 0.01f ? distance / maxSpeed : 0f;

        Vector3 futurePosition = targetPosition + targetVelocity * t;
        return Seek(position, velocity, futurePosition, maxSpeed, maxForce);
    }

    /// <summary>
    /// Huye prediciendo la posición futura del perseguidor.
    /// </summary>
    public static Vector3 Evade(Vector3 position, Vector3 velocity, Vector3 targetPosition, Vector3 targetVelocity, float maxSpeed, float maxForce)
    {
        float distance = Vector3.Distance(position, targetPosition);
        float t = maxSpeed > 0.01f ? distance / maxSpeed : 0f;

        Vector3 futurePosition = targetPosition + targetVelocity * t;
        return Flee(position, velocity, futurePosition, maxSpeed, maxForce);
    }
}
