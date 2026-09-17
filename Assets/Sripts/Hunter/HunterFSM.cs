using UnityEngine;

/// <summary>
/// Contexto del NPC Cazador. Contiene todos los datos compartidos entre estados
/// (energía, waypoints, rango de visión) y los métodos de movimiento/detección
/// que los estados usan. La FSM en sí (StateMachine) vive acá, pero cada
/// IState decide sus propias transiciones.
///
/// Sin bala/disparo: en Hunting el cazador persigue con Pursuit y "atrapa"
/// al boid por contacto (distancia < catchDistance).
/// </summary>
public class HunterFSM : MonoBehaviour
{
    [Header("Movimiento")]
    public float maxSpeed = 6f;
    public float maxForce = 12f;

    [Header("Energía")]
    public float maxEnergy = 100f;
    public float energyDrainPatrol = 5f;   // por segundo, mientras patrulla
    public float energyDrainHunting = 10f; // por segundo, mientras persigue
    public float restDuration = 4f;        // segundos mínimos de descanso en Idle
    public float energyRecoverRate = 20f;  // por segundo, mientras descansa

    [Header("Patrulla")]
    public Transform[] waypoints;
    public bool pingPong = true; // true: va y vuelve. false: vuelve al primero (loop).
    public float waypointThreshold = 0.5f;

    [Header("Caza")]
    public float visionRadius = 8f;
    public float loseSightMultiplier = 1.3f; // margen para "perder de vista" (histéresis)
    public float catchDistance = 1f;
    public LayerMask boidLayer;

    [Header("Límite de área (piso)")]
    public float containWeight = 3f;
    [Tooltip("Altura sobre la superficie del piso. Dejalo en 0 si este objeto es un pivote vacío y el modelo visual es un hijo desplazado hacia arriba.")]
    public float heightOffset = 0f;

    public float CurrentEnergy { get; private set; }
    public Vector3 Velocity { get; private set; }
    public Transform CurrentTarget { get; set; }

    // Estados expuestos para que cada IState pueda pedirle a la FSM cambiar a otro.
    public IdleState idleState;
    public PatrolState patrolState;
    public HuntingState huntingState;

    private StateMachine stateMachine;

    void Awake()
    {
        CurrentEnergy = maxEnergy;
        stateMachine = new StateMachine();

        idleState = new IdleState(this);
        patrolState = new PatrolState(this);
        huntingState = new HuntingState(this);
    }

    void Start()
    {
        stateMachine.ChangeState(patrolState);
    }

    void Update()
    {
        stateMachine.Update();
    }

    public void ChangeState(IState newState)
    {
        stateMachine.ChangeState(newState);
    }

    // ---------------------------------------------------------------
    // MOVIMIENTO
    // ---------------------------------------------------------------

    /// <summary>Movimiento simple hacia un punto fijo (usado en Patrol).</summary>
    public void MoveTo(Vector3 target)
    {
        Vector3 steering = SteeringBehaviors.Seek(transform.position, Velocity, target, maxSpeed, maxForce);
        ApplyMovement(steering);
    }

    /// <summary>Persecución con predicción de posición futura (usado en Hunting).</summary>
    public void PursuitTarget(Transform target)
    {
        Vector3 targetVelocity = Vector3.zero;
        Boid boid = target.GetComponent<Boid>();
        if (boid != null) targetVelocity = boid.velocity;

        Vector3 steering = SteeringBehaviors.Pursuit(transform.position, Velocity, target.position, targetVelocity, maxSpeed, maxForce);
        ApplyMovement(steering);
    }

    private void ApplyMovement(Vector3 steering)
    {
        steering.y = 0f;

        if (WorldBounds.Instance != null)
            steering += WorldBounds.Instance.Contain(transform.position, Velocity, maxSpeed, maxForce) * containWeight;

        Vector3 newVel = Velocity + steering * Time.deltaTime;
        newVel.y = 0f;
        Velocity = Vector3.ClampMagnitude(newVel, maxSpeed);

        transform.position += Velocity * Time.deltaTime;

        if (WorldBounds.Instance != null)
            transform.position = WorldBounds.Instance.ClampToArea(transform.position, heightOffset);

        if (Velocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(Velocity.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
    }

    public void Stop()
    {
        Velocity = Vector3.zero;
    }

    // ---------------------------------------------------------------
    // ENERGÍA
    // ---------------------------------------------------------------

    public void DrainEnergy(float ratePerSecond)
    {
        CurrentEnergy = Mathf.Max(0f, CurrentEnergy - ratePerSecond * Time.deltaTime);
    }

    public void RecoverEnergy()
    {
        CurrentEnergy = Mathf.Min(maxEnergy, CurrentEnergy + energyRecoverRate * Time.deltaTime);
    }

    // ---------------------------------------------------------------
    // DETECCIÓN
    // ---------------------------------------------------------------

    public Transform DetectClosestBoid()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, visionRadius, boidLayer);
        float closestDist = Mathf.Infinity;
        Transform closest = null;

        foreach (var h in hits)
        {
            float d = Vector3.Distance(transform.position, h.transform.position);
            if (d < closestDist)
            {
                closestDist = d;
                closest = h.transform;
            }
        }
        return closest;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRadius);
    }
}
