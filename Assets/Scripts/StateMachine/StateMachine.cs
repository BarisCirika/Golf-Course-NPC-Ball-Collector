using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine<T,R>
{
	public Action<R> StateChanged;

	private Dictionary<R, BaseState<T,R>> _states;
	private BaseState<T,R> _currentState;
	private T _playerController;

	public StateMachine(T playerController)
	{
		_playerController = playerController;
		_states = new Dictionary<R, BaseState<T,R>>();
	}

	public void AddState(R stateType, BaseState<T,R> state)
	{
        state.InitState(_playerController, this);
        _states.Add(stateType, state);
	}

	public void Initialize(R stateType)
	{
		if (_states.ContainsKey(stateType) == false)
		{
			Debug.LogWarning($"State Name: {stateType}, not found");
			return;
		}
		BaseState<T,R> startingState = _states[stateType];
		SetState(startingState);
	}
	
	public void SendMessage(int messageType, object message)
	{
		if(_currentState != null)
		{
			_currentState.OnMessageReceived(messageType, message);
		}
	}
	
	public void ForceChangeState(R stateType)
	{
		if(_states.ContainsKey(stateType) == false)
		{
			Debug.LogWarning($"State Name: {stateType}, not found");
			return;
		}
		
		BaseState<T,R> newState = _states[stateType];
		if (_currentState != null)
		{
			_currentState.Exit();
		}
		SetState(newState);
	}

	public void ChangeState(R stateType)
	{
		if(_states.ContainsKey(stateType) == false)
		{
			Debug.LogWarning($"State Name: {stateType}, not found");
			return;
		}

		BaseState<T,R> newState = _states[stateType];
		if (_currentState != null)
		{
			if (Convert.ToInt32(_currentState.GetStateType()) == Convert.ToInt32(newState.GetStateType()))
			{
				return;
			}
			//Debug.LogWarning("State changed From: " + _currentState.GetStateType() + ", To: " + newState.GetStateType());
			_currentState.Exit();
		}
		SetState(newState);
	}

	public void Tick()
	{
		if (_currentState != null) _currentState.Tick();
	}

	public void OnDestroy()
	{
		if (_currentState != null) _currentState.Exit();
	}

	private void SetState(BaseState<T,R> newState)
	{
		_currentState = newState;
		_currentState.Enter();
		StateChanged?.Invoke(_currentState.GetStateType());
	}

	public BaseState<T,R> GetCurrentState()
	{
		return _currentState;
	}

	public R GetCurrentStateType()
	{
		if (_currentState == null)
		{
			return default;
		}

		return _currentState.GetStateType();
	}
}