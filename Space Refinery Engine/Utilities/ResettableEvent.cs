namespace Space_Refinery_Engine;

public sealed record class ResettableEvent // check whether this could be a struct
{
	private Action? internalDelegate;

	public ResettableEvent(WeakEvent resetEvent)
	{
		resetEvent.Subscribe(Reset);
	}

	public ResettableEvent()
	{

	}

	public void Subscribe(Action eventHandler)
	{
		internalDelegate += eventHandler;
	}

	public void Reset()
	{
		internalDelegate = null;
	}

	public void Invoke()
	{
		internalDelegate?.Invoke();
	}

	public static ResettableEvent operator +(ResettableEvent resettableEvent, Action eventHandler)
	{
		resettableEvent.Subscribe(eventHandler);

		return resettableEvent;
	}
}

public sealed record class ResettableEvent<TParameter>
{
	private Action<TParameter>? internalDelegate;

	public ResettableEvent(WeakEvent resetEvent)
	{
		resetEvent.Subscribe(Reset);
	}

	public ResettableEvent()
	{

	}

	public void Subscribe(Action<TParameter> eventHandler)
	{
		internalDelegate += eventHandler;
	}

	public void Reset()
	{
		internalDelegate = null;
	}

	public void Invoke(TParameter parameter)
	{
		internalDelegate?.Invoke(parameter);
	}

	public static ResettableEvent<TParameter> operator +(ResettableEvent<TParameter> resettableEvent, Action<TParameter> eventHandler)
	{
		resettableEvent.Subscribe(eventHandler);

		return resettableEvent;
	}
}
