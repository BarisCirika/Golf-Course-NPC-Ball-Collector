
public abstract class BaseState<T,R>
{
	protected T _playerController;
	protected StateMachine<T,R> _stateMachine;
	protected R _stateType;

	public void InitState(T playerController, StateMachine<T,R> stateMachine)
	{
		_playerController = playerController;
		_stateMachine = stateMachine;
		_stateType = SetStateType();

		OnStateCreated();
	}

	protected virtual void OnStateCreated() { }

	public R GetStateType()
	{
		_stateType = SetStateType();
		return _stateType;
	}
	
	public virtual void OnMessageReceived(int messageType, object message){}

	protected abstract R SetStateType();
	public abstract void Enter();
	public abstract void Tick();
	public abstract void Exit();
}