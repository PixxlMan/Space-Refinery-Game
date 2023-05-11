using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Utilities;

public static class Convenience
{
	public static T SelectRandom<T>(this ICollection<T> collection, Random? random = null)
	{
		random ??= Random.Shared;

		return collection.ElementAt(random.Next(0, collection.Count));
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
