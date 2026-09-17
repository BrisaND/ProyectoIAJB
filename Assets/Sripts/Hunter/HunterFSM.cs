using UnityEngine;

public class HunterFSM : MonoBehaviour
{
    [Header("Movimiento")]
    public float maxSpeed = 6f;
    public float maxForce = 12f;

    [Header("Energía")]
    public float maxEnergy = 100f;
    public float energyDrainPatrol = 5f;
    public float energyDrainHunting = 10f;
    public float restDuration = 4f;
    public float energyRecoverRate = 20f;

    [Header("Patrulla")]
    public Transform[] waypoints;
    public bool pingPong = true;
    public float waypointThreshold = 0.5f;

    [Header("Caza")]
    public float visionRadius = 8f;
    public float loseSightMultiplier = 1.3f;
    public float catchDistance = 1f;
    public LayerMask boidLayer;

    [Header("Límite de área (piso)")]
    public float containWeight = 3f;

    public float CurrentEnergy { get; private set; }
    public Vector3 Velocity { get; private set; }
    public Transform CurrentTarget { get; set; }

    public IdleState idleState;
    public PatrolState patrolState;
    public HuntingState huntingState;

    private StateMachine stateMachine;
    private float autoHeightOffset;

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
        // Auto-detectar la distancia del pivote a la base del modelo 3D
        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend != null)
            autoHeightOffset = transform.position.y - rend.bounds.min.y;

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

    public void MoveTo(Vector3 target)
    {
        Vector3 steering = SteeringBehaviors.Seek(transform.position, Velocity, target, maxSpeed, maxForce);
        ApplyMovement(steering);
    }

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

        Vector3 currentVel = Velocity + steering * Time.deltaTime;
        currentVel.y = 0f;
        Velocity = Vector3.ClampMagnitude(currentVel, maxSpeed);

        transform.position += Velocity * Time.deltaTime;

        if (WorldBounds.Instance != null)
            transform.position = WorldBounds.Instance.ClampToArea(transform.position, autoHeightOffset);

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

    public void DrainEnergy(float ratePerSecond)
    {
        CurrentEnergy = Mathf.Max(0f, CurrentEnergy - ratePerSecond * Time.deltaTime);
    }

    public void RecoverEnergy()
    {
        CurrentEnergy = Mathf.Min(maxEnergy, CurrentEnergy + energyRecoverRate * Time.deltaTime);
    }

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

    public string CurrentStateName => stateMachine?.CurrentState != null
    ? stateMachine.CurrentState.GetType().Name.Replace("State", "")
    : "Ninguno";

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRadius);
    }
}