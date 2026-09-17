using UnityEngine;

/// <summary>
/// El cazador persigue a hunter.CurrentTarget usando Pursuit (predicción de
/// posición futura). Si atrapa al boid (distancia &lt; catchDistance) lo
/// "caza" y vuelve a Patrol. Si pierde de vista al boid, vuelve a Patrol.
/// Si se queda sin energía, vuelve a Idle.
///
/// No hay disparo/bala en esta versión: la captura es siempre por contacto.
/// </summary>
public class HuntingState : IState
{
    private readonly HunterFSM hunter;

    public HuntingState(HunterFSM hunter)
    {
        this.hunter = hunter;
    }

    public void Enter() { }

    public void Execute()
    {
        Transform target = hunter.CurrentTarget;

        // El boid puede haber sido destruido (comido, atrapado, etc.) o alejado.
        if (target == null)
        {
            hunter.ChangeState(hunter.patrolState);
            return;
        }

        float distance = Vector3.Distance(hunter.transform.position, target.position);

        if (distance > hunter.visionRadius * hunter.loseSightMultiplier)
        {
            hunter.CurrentTarget = null;
            hunter.ChangeState(hunter.patrolState);
            return;
        }

        hunter.PursuitTarget(target);
        hunter.DrainEnergy(hunter.energyDrainHunting);

        if (distance < hunter.catchDistance)
        {
            Object.Destroy(target.gameObject);
            hunter.CurrentTarget = null;
            hunter.ChangeState(hunter.patrolState);
            return;
        }

        if (hunter.CurrentEnergy <= 0f)
        {
            hunter.CurrentTarget = null;
            hunter.ChangeState(hunter.idleState);
        }
    }

    public void Exit() { }
}
