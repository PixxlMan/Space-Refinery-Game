using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Space_Refinery_Game
{
	public class SerializationReferenceHandler
	{
		public ISerializableReference this[Guid guid]
		{
			get { lock (SyncRoot) return guidToSerializableReference[guid]; }
		}

		public object SyncRoot = new();

		private Dictionary<Guid, ISerializableReference> guidToSerializableReference = new();

		private Dictionary<Guid, List<YieldAwaitable>> awaitedEventuallyResolvedReferences = new();

		public bool ContainsReference(ISerializableReference serializableReference)
		{
			lock (SyncRoot)
			{
				return guidToSerializableReference.ContainsKey(serializableReference.SerializableReferenceGUID);
			}
		}

		public bool ContainsReference(Guid guid)
		{
			lock (SyncRoot)
			{
				return guidToSerializableReference.ContainsKey(guid);
			}
		}

		public void RegisterReference(ISerializableReference serializableReference)
		{
			lock (SyncRoot)
			{
				if (serializableReference.SerializableReferenceGUID == Guid.Empty)
				{
					throw new ArgumentException($"The GUID of this {nameof(ISerializableReference)} is not initialized!", nameof(serializableReference));
				}

				if (awaitedEventuallyResolvedReferences.ContainsKey(serializableReference.SerializableReferenceGUID))
				{
					AwaitYieldAwaitables(awaitedEventuallyResolvedReferences[serializableReference.SerializableReferenceGUID]);
				}

				guidToSerializableReference.Add(serializableReference.SerializableReferenceGUID, serializableReference);
			}

			async static void AwaitYieldAwaitables(List<YieldAwaitable> awaitables)
			{
				foreach (var awaitable in awaitables)
				{
					await awaitable;
				}
			}
		}

		public async Task<ISerializableReference> AwaitEventualReference(Guid guid)
		{
			lock (SyncRoot)
			{
				if (ContainsReference(guid))
				{
					return this[guid];
				}
				else
				{
					if (awaitedEventuallyResolvedReferences.ContainsKey(guid))
					{
						awaitedEventuallyResolvedReferences[guid].Add(Task.Yield());
					}
					else
					{
						awaitedEventuallyResolvedReferences.Add(guid, new List<YieldAwaitable>() { Task.Yield() });
					}

					return this[guid];
				}
			}
		}

		public void Serialize(XmlWriter writer)
		{
			writer.Serialize(guidToSerializableReference.Values, (w, s) => w.SerializeWithEmbeddedType(s));
		}
		
		public static SerializationReferenceHandler Deserialize(XmlReader reader, GameData gameData)
		{
			SerializationReferenceHandler referenceHandler = new();

			reader.DeserializeCollection((r) => referenceHandler.RegisterReference((ISerializableReference)r.DeserializeEntitySerializableWithEmbeddedType(gameData, referenceHandler)));

			return referenceHandler;
		}
	}
}
