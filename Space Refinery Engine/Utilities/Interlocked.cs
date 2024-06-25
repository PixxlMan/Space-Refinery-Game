namespace Space_Refinery_Engine;

public struct Interlocked<T> where T : class
{
	public unsafe T ValueField;

	public T Value
	{
		get => Interlocked.CompareExchange(ref ValueField, ValueField, ValueField);
		set => Interlocked.Exchange(ref ValueField, value);
	}
}

public struct InterlockedInt
{
	public unsafe int ValueField;

	public int Value
	{
		get => Volatile.Read(ref ValueField);
	}
	
	public void Set(int value)
	{
		Interlocked.Exchange(ref ValueField, value);
	}

	public void Increment()
	{
		Interlocked.Increment(ref ValueField);
	}

	public void Decrement()
	{
		Interlocked.Decrement(ref ValueField);
	}

	public void Add(int value)
	{
		Interlocked.Add(ref ValueField, value);
	}

	public void Subtract(int value)
	{
		Interlocked.Add(ref ValueField, -value);
	}
}
