namespace Space_Refinery_Utilities;

public static class Convenience
{
	public static T SelectRandom<T>(this ICollection<T> collection, Random? random = null)
	{
		if (collection.Count == 0)
		{
			throw new Exception("Cannot select an element when the collection is empty");
		}

		random ??= Random.Shared;

		int nextElement = random.Next(0, collection.Count);

		return collection.ElementAt(nextElement);
	}
	
	public static T SelectRandomNew<T>(this ICollection<T> collection, T current, Random? random = null)
	{
		random ??= Random.Shared;

		T toReturn = current;
		while (object.ReferenceEquals(toReturn, current))
		{
			toReturn = collection.ElementAt(random.Next(0, collection.Count));
		}

		return toReturn;
	}
}
