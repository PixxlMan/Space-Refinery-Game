using System.Diagnostics;

namespace Space_Refinery_Game
{
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
				throw new Exception(exceptionText ?? $"Item does not exist on this {nameof(HashSet<T>)}.");
			}
		}
	}
}
