using UnityEngine;

/// <summary>
/// El cazador regresa al último punto de patrulla. Una vez allí,
/// se detiene y recupera energía. Después de restDuration segundos
/// Y con la energía llena, vuelve a Patrol.
/// </summary>
public class RestState : IState
{
    private readonly HunterFSM hunter;
    private float restTimer;

    public RestState(HunterFSM hunter)
    {
        this.hunter = hunter;
    }

    public void Enter()
    {
        restTimer = 0f;
    }

    public void Execute()
    {
        Vector3 targetPos = hunter.LastPatrolPoint;
        targetPos.y = hunter.transform.position.y;

        Vector3 hunterXZ = new Vector3(hunter.transform.position.x, 0f, hunter.transform.position.z);
        Vector3 targetXZ = new Vector3(targetPos.x, 0f, targetPos.z);

        // Si aún no llegó al waypoint de descanso, camina hacia él
        if (Vector3.Distance(hunterXZ, targetXZ) > hunter.waypointThreshold)
        {
            hunter.MoveTo(targetPos);
        }
        else
        {
            // Al llegar al punto, se detiene y recupera energía
            hunter.Stop();
            hunter.RecoverEnergy();
            restTimer += Time.deltaTime;

            if (restTimer >= hunter.restDuration && hunter.CurrentEnergy >= hunter.maxEnergy)
            {
                hunter.ChangeState(hunter.patrolState);
            }
        }
    }

    public void Exit() { }
}