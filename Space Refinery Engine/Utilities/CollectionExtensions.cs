using System.Diagnostics;
using System.Collections.Concurrent;

namespace Space_Refinery_Engine;

public static class CollectionExtensions
{
	[DebuggerHidden]
	public static void AddUnique<T>(this HashSet<T> hashSet, T item, string? exceptionText = null)
	{
		if (!hashSet.Add(item))
		{
			throw new Exception(exceptionText ?? $"Item has already been added to this {nameof(HashSet<T>)}. It is not unique.");
		}
	}

	[DebuggerHidden]
	public static void RemoveStrict<T>(this HashSet<T> hashSet, T item, string? exceptionText = null)
	{
		if (!hashSet.Remove(item))
		{
			throw new Exception(exceptionText ?? $"Item does not exist in this {nameof(HashSet<T>)}.");
		}
	}

	[DebuggerHidden]
	public static void AddUnique<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, TValue value, string? exceptionText = null)
	{
		if (!dictionary.TryAdd(key, value))
		{
			throw new Exception(exceptionText ?? $"Key has already been added to this {nameof(ConcurrentDictionary<TKey, TValue>)}. It is not unique.");
		}
	}

	[DebuggerHidden]
	public static void RemoveStrict<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, string? exceptionText = null)
	{
		if (!dictionary.Remove(key, out _))
		{
			throw new Exception(exceptionText ?? $"Key does not exist in this {nameof(ConcurrentDictionary<TKey, TValue>)}.");
		}
	}

	[DebuggerHidden]
	public static void AddUnique<TKey>(this ConcurrentDictionary<TKey, EmptyType> dictionary, TKey key, string? exceptionText = null)
	{
		if (!dictionary.TryAdd(key, default))
		{
			throw new Exception(exceptionText ?? $"Key has already been added to this {nameof(ConcurrentDictionary<TKey, EmptyType>)}. It is not unique.");
		}
	}

	[DebuggerHidden]
	public static void RemoveStrict<TKey>(this ConcurrentDictionary<TKey, EmptyType> dictionary, TKey key, string? exceptionText = null)
	{
		if (!dictionary.Remove(key, out _))
		{
			throw new Exception(exceptionText ?? $"Key does not exist in this {nameof(ConcurrentDictionary<TKey, EmptyType>)}.");
		}
	}
}
