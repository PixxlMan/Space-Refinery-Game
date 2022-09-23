using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Refinery_Game
{
	public class SerializationReferenceHandler
	{
		public ISerializableReference this[Guid guid]
		{
			get => guidToSerializableReference[guid];
		}

		private Dictionary<Guid, ISerializableReference> guidToSerializableReference = new();

		public bool ContainsReference(ISerializableReference serializableReference)
		{
			return guidToSerializableReference.ContainsKey(serializableReference.SerializableReferenceGUID);
		}

		public void RegisterReference(ISerializableReference serializableReference)
		{
			if (serializableReference.SerializableReferenceGUID == Guid.Empty)
			{
				throw new ArgumentException($"The GUID of this {nameof(ISerializableReference)} is not initialized!", nameof(serializableReference));
			}

			guidToSerializableReference.Add(serializableReference.SerializableReferenceGUID, serializableReference);
		}
	}
}
