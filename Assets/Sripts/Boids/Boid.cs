using UnityEngine;

/// <summary>
/// Las 4 acciones posibles que puede elegir el Decision Tree de un boid.
/// </summary>
public enum BoidAction
{
    SeekFood,
    EvadeHunter,
    Flock,
    Wander
}

/// <summary>
/// Agente autónomo (boid). Se mueve SIN Rigidbody: la posición se actualiza
/// a mano en Update() usando la velocity calculada por los Steering Behaviors.
///
/// Cada frame:
///   1) Detecta el entorno (comida cercana, cazador en rango, vecinos).
///   2) El Decision Tree (DecideAction) elige UNA acción según prioridad.
///   3) Se aplica el Steering Behavior correspondiente a esa acción.
/// </summary>
[DisallowMultipleComponent]
public class Boid : MonoBehaviour
{
    [Header("Movimiento")]
    public float maxSpeed = 5f;
    public float maxForce = 10f;

    [Header("Flocking")]
    public float separationRadius = 1.5f;
    public float alignmentRadius = 3f;
    public float cohesionRadius = 4f;
    public float separationWeight = 1.5f;
    public float alignmentWeight = 1f;
    public float cohesionWeight = 1f;

    [Header("Detección")]
    public float foodDetectionRadius = 5f;
    public float hunterVisionRadius = 6f;
    public LayerMask foodLayer;
    public LayerMask hunterLayer;
    public float eatDistance = 0.5f;

    [Header("Wander (sin comida, sin cazador, sin vecinos)")]
    public float wanderJitter = 2f;
    public float wanderRadius = 2f;
    public float wanderDistance = 3f;

    [Header("Límite de área (piso)")]
    [Tooltip("Peso extra para el steering que lo mantiene dentro del área definida por WorldBounds.")]
    public float containWeight = 3f;
    [Tooltip("Altura sobre la superficie del piso. Dejalo en 0 si este objeto es un pivote vacío y el modelo visual es un hijo desplazado hacia arriba.")]
    public float heightOffset = 0f;

    // Velocidad actual, expuesta para que el Hunter pueda predecir la posición futura (Pursuit).
    public Vector3 velocity { get; private set; }

    private Transform targetFood;
    private Transform hunter;
    private BoidAction currentAction;
    private Vector3 wanderTarget;

    void Start()
    {
        velocity = transform.forward * (maxSpeed * 0.5f);
        if (FlockManager.Instance != null)
            FlockManager.Instance.Register(this);

        // Punto inicial de wander, en un círculo alrededor del boid.
        wanderTarget = Random.insideUnitSphere * wanderRadius;
        wanderTarget.y = 0f;
    }

    void OnDestroy()
    {
        if (FlockManager.Instance != null)
            FlockManager.Instance.Unregister(this);
    }

    void Update()
    {
        DetectEnvironment();
        currentAction = DecideAction();

        Vector3 steering = Vector3.zero;

        switch (currentAction)
        {
            case BoidAction.SeekFood:
                steering = ExecuteSeekFood();
                break;

            case BoidAction.EvadeHunter:
                steering = ExecuteEvadeHunter();
                break;

            case BoidAction.Flock:
                steering = ExecuteFlock();
                break;

            case BoidAction.Wander:
                steering = ExecuteWander();
                break;
        }

        // El límite del área tiene prioridad sobre cualquier otra acción:
        // se suma siempre, incluso mientras persigue comida o escapa del cazador.
        if (WorldBounds.Instance != null)
            steering += WorldBounds.Instance.Contain(transform.position, velocity, maxSpeed, maxForce) * containWeight;

        ApplyMovement(steering);
    }

    private void ApplyMovement(Vector3 steering)
    {
        steering.y = 0f; // Anula la fuerza vertical

        // Usamos una variable local para modificar componentes de Vector3
        Vector3 currentVel = velocity + steering * Time.deltaTime;
        currentVel.y = 0f; // Mantiene el movimiento horizontal
        velocity = Vector3.ClampMagnitude(currentVel, maxSpeed);

        transform.position += velocity * Time.deltaTime;

        // Red de seguridad dentro de los límites del mapa
        if (WorldBounds.Instance != null)
            transform.position = WorldBounds.Instance.ClampToArea(transform.position, heightOffset);

        if (velocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
    }

    // ---------------------------------------------------------------
    // DETECCIÓN DEL ENTORNO
    // ---------------------------------------------------------------

    private void DetectEnvironment()
    {
        targetFood = FindClosest(foodDetectionRadius, foodLayer);
        hunter = FindClosest(hunterVisionRadius, hunterLayer);
    }

    private Transform FindClosest(float radius, LayerMask layer)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, layer);
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

    // ---------------------------------------------------------------
    // DECISION TREE
    //
    //  ¿Hay comida cerca?
    //      SI  -> SeekFood (Arrive)
    //      NO  -> ¿Hay cazador en rango de visión?
    //              SI  -> EvadeHunter (Evade)
    //              NO  -> ¿Hay otros boids cerca?
    //                      SI  -> Flock (Separación + Alineación + Cohesión)
    //                      NO  -> Wander (movimiento aleatorio)
    // ---------------------------------------------------------------

    private BoidAction DecideAction()
    {
        if (targetFood != null)
            return BoidAction.SeekFood;

        if (hunter != null)
            return BoidAction.EvadeHunter;

        if (FlockManager.Instance != null && FlockManager.Instance.GetNeighbors(this, cohesionRadius).Count > 0)
            return BoidAction.Flock;

        return BoidAction.Wander;
    }

    // ---------------------------------------------------------------
    // EJECUCIÓN DE CADA ACCIÓN
    // ---------------------------------------------------------------

    private Vector3 ExecuteSeekFood()
    {
        Vector3 steer = SteeringBehaviors.Arrive(transform.position, velocity, targetFood.position, maxSpeed, maxForce, foodDetectionRadius);

        if (Vector3.Distance(transform.position, targetFood.position) < eatDistance)
        {
            Food food = targetFood.GetComponent<Food>();
            if (food != null) food.Consume();
            targetFood = null;
        }
        return steer;
    }

    private Vector3 ExecuteEvadeHunter()
    {
        Vector3 hunterVelocity = Vector3.zero;
        HunterFSM hunterFSM = hunter.GetComponent<HunterFSM>();
        if (hunterFSM != null) hunterVelocity = hunterFSM.Velocity;

        return SteeringBehaviors.Evade(transform.position, velocity, hunter.position, hunterVelocity, maxSpeed, maxForce);
    }

    private Vector3 ExecuteFlock()
    {
        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
        int sepCount = 0, aliCount = 0, cohCount = 0;

        foreach (Boid other in FlockManager.Instance.Boids)
        {
            if (other == this) continue;
            float d = Vector3.Distance(transform.position, other.transform.position);

            // Separación: alejarse de vecinos muy cercanos.
            if (d < separationRadius && d > 0.001f)
            {
                separation += (transform.position - other.transform.position).normalized / d;
                sepCount++;
            }

            // Alineación: promediar la dirección de vecinos cercanos.
            if (d < alignmentRadius)
            {
                alignment += other.velocity;
                aliCount++;
            }

            // Cohesión: acercarse al centro de masa de los vecinos.
            if (d < cohesionRadius)
            {
                cohesion += other.transform.position;
                cohCount++;
            }
        }

        Vector3 steering = Vector3.zero;

        if (sepCount > 0)
        {
            separation /= sepCount;
            Vector3 sepTarget = transform.position + separation;
            steering += SteeringBehaviors.Seek(transform.position, velocity, sepTarget, maxSpeed, maxForce) * separationWeight;
        }

        if (aliCount > 0)
        {
            alignment /= aliCount;
            Vector3 alignSteer = Vector3.ClampMagnitude(alignment - velocity, maxForce);
            steering += alignSteer * alignmentWeight;
        }

        if (cohCount > 0)
        {
            cohesion /= cohCount;
            steering += SteeringBehaviors.Seek(transform.position, velocity, cohesion, maxSpeed, maxForce) * cohesionWeight;
        }

        return steering;
    }

    private Vector3 ExecuteWander()
    {
        // Pequeño offset aleatorio sobre un círculo proyectado adelante del boid.
        wanderTarget += new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)) * wanderJitter * Time.deltaTime;
        wanderTarget = wanderTarget.normalized * wanderRadius;

        Vector3 circleCenter = transform.position + transform.forward * wanderDistance;
        Vector3 target = circleCenter + wanderTarget;

        return SteeringBehaviors.Seek(transform.position, velocity, target, maxSpeed, maxForce);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, foodDetectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hunterVisionRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, cohesionRadius);
    }
}
