/// <summary>
/// Máquina de Estados Finita genérica. No depende de Unity ni del Hunter:
/// solo administra IState. Las transiciones las decide cada estado (según
/// la consigna, "la transición de los estados deberá ser aplicada por los
/// estados mismos"), llamando a ChangeState desde su propio Execute().
/// </summary>
public class StateMachine
{
    public IState CurrentState { get; private set; }

    public void ChangeState(IState newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState?.Enter();
    }

    public void Update()
    {
        CurrentState?.Execute();
    }
}
