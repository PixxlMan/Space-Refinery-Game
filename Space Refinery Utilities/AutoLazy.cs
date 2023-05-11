using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Utilities;

/// <summary>
/// Unlike <c>System.Lazy<T></c> this contains an implicit conversion to <c>T</c>, making for an easy (lazy, even ;) ) drop in replacement.
/// </summary>
public class AutoLazy<T> : Lazy<T>
{
	public AutoLazy(Func<T> factory) : base (factory)
	{

	}

	public AutoLazy(Func<T> factory, bool isThreadSafe) : base (factory, isThreadSafe)
	{

	}

	public static implicit operator T(AutoLazy<T> lazyT)
	{
		return lazyT.Value;
	}

	/*public static implicit operator AutoLazy<T>(Func<T> factory)
	{
		return new(factory);
	}*/
}
