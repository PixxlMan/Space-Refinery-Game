namespace Space_Refinery_Engine;

/// <summary>
/// An event type that has only a weak reference to the types it calls, allowing them to be reclaimed by the garbage collector.
/// </summary>
/// <remarks>
/// This class is thread safe.
/// </remarks>
public sealed class WeakEvent // Consider something like this: https://github.com/StephenClearyArchive/Nito.KitchenSink/blob/d5f7d6dfdaf6642868ecd4cd81d8c0c90af55ca7/Source/Nito.KitchenSink/Weakness/WeakCollection.cs
{
	private object syncRoot = new();

	private List<WeakReference<Action>> invocationList = new();

	public WeakEvent(WeakEvent purgeEvent)
	{
		purgeEvent.Subscribe(Purge);
	}

	public WeakEvent()
	{
	}

	/// <summary>
	/// Purges dead references from the invocation list.
	/// This method must be called periodically, ideally when there likely references which are now dead, to prevent memory buildup.
	/// </summary>
	public void Purge()
	{
		lock (syncRoot)
		{
			List<WeakReference<Action>> aliveReferences = new();

			foreach (var weakRefInvocation in invocationList)
			{
				if (weakRefInvocation.TryGetTarget(out _))
				{
					aliveReferences.Add(weakRefInvocation);
				}
			}

			invocationList = aliveReferences;
		}
	}

	public void Subscribe(Action eventHandler)
	{
		lock (syncRoot)
		{
			invocationList.Add(new(eventHandler));
		}
	}

	public void Invoke()
	{
		lock (syncRoot)
		{
			foreach (var weakRefInvocation in invocationList)
			{
				if (weakRefInvocation.TryGetTarget(out Action? eventListener))
				{
					eventListener.Invoke();
				}
			}
		}
	}

	public void InvokeAndPurge()
	{
		lock (syncRoot)
		{
			List<WeakReference<Action>> aliveReferences = new();

			foreach (var weakRefInvocation in invocationList)
			{
				if (weakRefInvocation.TryGetTarget(out Action? eventListener))
				{
					eventListener.Invoke();

					aliveReferences.Add(weakRefInvocation);
				}
			}

			invocationList = aliveReferences;
		}
	}

	public static WeakEvent operator +(WeakEvent weakEvent, Action eventHandler)
	{
		if (weakEvent is null)
		{
			weakEvent = new();
		}

		weakEvent.Subscribe(eventHandler);

		return weakEvent;
	}
}

/// <summary>
/// An event type that has only a weak reference to the types it calls, allowing them to be reclaimed by the garbage collector.
/// </summary>
/// <remarks>
/// This class is thread safe.
/// </remarks>
public sealed class WeakEvent<TParameter>
{
	private object syncRoot = new();

	private List<WeakReference<Action<TParameter>>> invocationList = new();

	public WeakEvent(WeakEvent purgeEvent)
	{
		purgeEvent.Subscribe(Purge);
	}

	public WeakEvent()
	{
	}

	/// <summary>
	/// Purges dead references from the invocation list.
	/// This method must be called periodically, ideally when there likely references which are now dead, to prevent memory buildup.
	/// </summary>
	public void Purge()
	{
		lock (syncRoot)
		{
			List<WeakReference<Action<TParameter>>> aliveReferences = new();

			foreach (var weakRefInvocation in invocationList)
			{
				if (weakRefInvocation.TryGetTarget(out _))
				{
					aliveReferences.Add(weakRefInvocation);
				}
			}

			invocationList = aliveReferences;
		}
	}

	public void Subscribe(Action<TParameter> eventHandler)
	{
		lock (syncRoot)
		{
			invocationList.Add(new(eventHandler));
		}
	}

	public void Invoke(TParameter parameter)
	{
		lock (syncRoot)
		{
			if (invocationList.Count == 0)
			{
				return;
			}

			foreach (var weakRefInvocation in invocationList)
			{
				if (weakRefInvocation.TryGetTarget(out Action<TParameter>? eventListener))
				{
					eventListener.Invoke(parameter);
				}
			}
		}
	}

	public void InvokeAndPurge(TParameter parameter)
	{
		lock (syncRoot)
		{
			if (invocationList.Count == 0)
			{
				return;
			}

			List<WeakReference<Action<TParameter>>> aliveReferences = new();

			foreach (var weakRefInvocation in invocationList)
			{
				if (weakRefInvocation.TryGetTarget(out Action<TParameter>? eventListener))
				{
					eventListener.Invoke(parameter);

					aliveReferences.Add(weakRefInvocation);
				}
			}

			invocationList = aliveReferences;
		}
	}

	// are these methods really thread safe? probably not right, since they're replacing themselves? hmm
	// maybe an WeakEvent.Event that has overriden add and remove operators perhaps could satisfy the functionality of this
	// without the messing with overwriting a reference
	// although the reference is identical, so it might not matter anyways...
	public static WeakEvent<TParameter> operator +(WeakEvent<TParameter> weakEvent, Action<TParameter> eventHandler)
	{
		if (weakEvent is null)
		{
			weakEvent = new();
		}

		weakEvent.Subscribe(eventHandler);

		return weakEvent;
	}
}
