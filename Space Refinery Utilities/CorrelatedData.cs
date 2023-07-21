//#if DEBUG
//#define IncludeUnits
//#endif

//using FixedPrecision;
//using Microsoft.Toolkit.HighPerformance;
//using System.Runtime.CompilerServices;

//namespace Space_Refinery_Utilities;

///// <summary>
///// Provides a way to represent and query data that is correlated in two axis.
///// </summary>
//public unsafe class CorrelatedData<TX, TY>
//#if IncludeUnits
//	where TY : struct, IUnit<TY>
//	where TX : struct, IUnit<TX>
//#else
//	where TY : IFixedPrecisionNumeral<IFixedPrecisionNumeral>
//	where TX : IFixedPrecisionNumeral<IFixedPrecisionNumeral>
//#endif
//{
//	private Memory2D<TX> txMemory;
//	private Memory2D<TY> tyMemory;

//	public CorrelatedData(uint size)
//	{
//		if (Unsafe.SizeOf<TX>() != Unsafe.SizeOf<TY>())
//		{
//			throw new InvalidOperationException($"Attempted to create a {nameof(CorrelatedData<TX, TY>)} type with TX and TY types that are not equal in size.");
//		}

//		txMemory = new TX[size, size];
//		tyMemory = txMemory;
//	}
//}
