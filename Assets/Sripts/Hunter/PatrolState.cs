using UnityEngine;

/// <summary>
/// El cazador recorre los waypoints en orden. Al llegar al último,
/// vuelve al primero (loop) o invierte el sentido (ping-pong), según
/// hunter.pingPong. Gasta energía; si llega a 0 pasa a Idle. Si detecta
/// un boid en su rango de visión, pasa a Hunting.
/// </summary>
public class PatrolState : IState
{
    private readonly HunterFSM hunter;
    private int currentIndex;
    private int direction = 1;

    public PatrolState(HunterFSM hunter)
    {
        this.hunter = hunter;
    }

    public void Enter()
    {
        // No reseteamos currentIndex: el cazador retoma la patrulla
        // desde donde la dejó la última vez.
    }

    public void Execute()
    {
        if (hunter.waypoints == null || hunter.waypoints.Length == 0)
            return;

        Transform target = hunter.waypoints[currentIndex];
        hunter.MoveTo(target.position);
        hunter.DrainEnergy(hunter.energyDrainPatrol);

        if (Vector3.Distance(hunter.transform.position, target.position) < hunter.waypointThreshold)
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
