using UnityEngine;

public class PatrolState : IState
{
    private readonly HunterFSM hunter;
    private int currentIndex;
    private int direction = 1;

    public PatrolState(HunterFSM hunter)
    {
        this.hunter = hunter;
    }

    public void Enter() { }

    public void Execute()
    {
        if (hunter.waypoints == null || hunter.waypoints.Length == 0)
            return;

        Transform target = hunter.waypoints[currentIndex];

        // Moverse hacia el objetivo manteniendo la misma altura del cazador
        Vector3 targetPos = target.position;
        targetPos.y = hunter.transform.position.y;

        hunter.MoveTo(targetPos);
        hunter.DrainEnergy(hunter.energyDrainPatrol);

        // Cálculo de distancia ignorando el eje Y
        Vector3 hunterXZ = new Vector3(hunter.transform.position.x, 0f, hunter.transform.position.z);
        Vector3 targetXZ = new Vector3(target.position.x, 0f, target.position.z);

        if (Vector3.Distance(hunterXZ, targetXZ) < hunter.waypointThreshold)
            AdvanceWaypoint();

        if (hunter.CurrentEnergy <= 0f)
        {
            hunter.ChangeState(hunter.idleState);
            return;
        }

        Transform boid = hunter.DetectClosestBoid();
        if (boid != null)
        {
            hunter.CurrentTarget = boid;
            hunter.ChangeState(hunter.huntingState);
        }
    }

    public void Exit() { }

    private void AdvanceWaypoint()
    {
        if (hunter.waypoints.Length <= 1) return;

        if (hunter.pingPong)
        {
            if (currentIndex + direction >= hunter.waypoints.Length || currentIndex + direction < 0)
                direction *= -1;

            currentIndex += direction;
        }
        else
        {
            currentIndex = (currentIndex + 1) % hunter.waypoints.Length;
        }
    }
}