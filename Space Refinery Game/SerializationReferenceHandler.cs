using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Space_Refinery_Game
{
	public sealed unsafe class SerializationReferenceHandler
	{
		public ISerializableReference this[Guid guid]
		{
			get { lock (SyncRoot) return guidToSerializableReference[guid]; }
		}

		public object SyncRoot = new();

		private Dictionary<Guid, ISerializableReference> guidToSerializableReference = new();

		private Dictionary<Guid, List<Action<ISerializableReference>>> eventualReferencesToFulfill = new();

		public bool AllowUnresolvedEventualReferences { get; private set; }

		public void EnterAllowEventualReferenceMode()
		{
			lock (SyncRoot)
			{
				AllowUnresolvedEventualReferences = true;
			}
		}

		public void ExitAllowEventualReferenceMode()
		{
			lock (SyncRoot)
			{
				AllowUnresolvedEventualReferences = false;

				if (eventualReferencesToFulfill.Count > 0)
				{
					throw new Exception("Not all eventual references have been resolved yet! Either this was called too early, or there is a missing reference.");
				}
			}
		}

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

				if (AllowUnresolvedEventualReferences)
				{
					if (eventualReferencesToFulfill.ContainsKey(serializableReference.SerializableReferenceGUID))
					{
						foreach (var eventualReferenceCallback in eventualReferencesToFulfill[serializableReference.SerializableReferenceGUID])
						{
							eventualReferenceCallback(serializableReference);
						}

						eventualReferencesToFulfill.Remove(serializableReference.SerializableReferenceGUID);
					}
				}

				guidToSerializableReference.Add(serializableReference.SerializableReferenceGUID, serializableReference);
			}
		}

		public void GetEventualReference(Guid guid, Action<ISerializableReference> referenceRegisteredCallback)
		{
			lock (SyncRoot)
			{
				if (ContainsReference(guid))
				{
					referenceRegisteredCallback(this[guid]);
				}
				else
				{
					if (!AllowUnresolvedEventualReferences)
					{
						throw new InvalidOperationException($"Cannot use eventual references when {nameof(AllowUnresolvedEventualReferences)} mode is not active!");
					}

					if (eventualReferencesToFulfill.ContainsKey(guid))
					{
						eventualReferencesToFulfill[guid].Add(referenceRegisteredCallback);
					}
					else
					{
						eventualReferencesToFulfill.TryAdd(guid, new List<Action<ISerializableReference>>() { referenceRegisteredCallback });
					}
				}
			}
		}

		public void Serialize(XmlWriter writer)
		{
			lock (SyncRoot)
			{
				if (AllowUnresolvedEventualReferences)
				{
					throw new InvalidOperationException($"Cannot serialize when {nameof(AllowUnresolvedEventualReferences)} mode is active!");
				}

				writer.Serialize(guidToSerializableReference.Values, (w, s) => w.SerializeWithEmbeddedType(s), nameof(SerializationReferenceHandler));
			}
		}
		
		public static SerializationReferenceHandler Deserialize(XmlReader reader, SerializationData serializationData)
		{
			SerializationReferenceHandler referenceHandler = new();

			referenceHandler.EnterAllowEventualReferenceMode();

			reader.DeserializeCollection((r) => referenceHandler.RegisterReference((ISerializableReference)r.DeserializeEntitySerializableWithEmbeddedType(serializationData, referenceHandler)), nameof(SerializationReferenceHandler));

			referenceHandler.ExitAllowEventualReferenceMode();

			return referenceHandler;
		}

		public void DeserializeInto(XmlReader reader, SerializationData serializationData)
		{
			EnterAllowEventualReferenceMode();

			reader.DeserializeCollection((r) => RegisterReference((ISerializableReference)r.DeserializeEntitySerializableWithEmbeddedType(serializationData, this)), nameof(SerializationReferenceHandler));

			ExitAllowEventualReferenceMode();
		}
	}
}
