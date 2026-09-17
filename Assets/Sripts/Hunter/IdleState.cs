using UnityEngine;

/// <summary>
/// El cazador se detiene y recupera energía. Después de restDuration segundos
/// Y con la energía llena, vuelve a Patrol.
/// </summary>
public class IdleState : IState
{
    private readonly HunterFSM hunter;
    private float restTimer;

    public IdleState(HunterFSM hunter)
    {
        this.hunter = hunter;
    }

    public void Enter()
    {
        restTimer = 0f;
        hunter.Stop();
    }

    public void Execute()
    {
        restTimer += Time.deltaTime;
        hunter.RecoverEnergy();

        if (restTimer >= hunter.restDuration && hunter.CurrentEnergy >= hunter.maxEnergy)
        {
            hunter.ChangeState(hunter.patrolState);
        }
    }

    public void Exit() { }
}
