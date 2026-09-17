/// <summary>
/// Contrato que debe cumplir cualquier estado de una FSM.
/// </summary>
public interface IState
{
    void Enter();
    void Execute();
    void Exit();
}
