using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Space_Refinery_Game;

public sealed class SerializationReferenceHandler
{
	public ISerializableReference this[Guid guid]
	{
		get { lock (SyncRoot) return serializableReferenceLookup[guid]; }
	}

	public ISerializableReference this[string referenceName]
	{
		get { lock (SyncRoot) return serializableReferenceLookup[referenceName]; }
	}

	public ISerializableReference this[SerializableReference reference]
	{
		get { lock (SyncRoot) return serializableReferenceLookup[reference]; }
	}

	public int ReferenceCount { get { lock (SyncRoot) return serializableReferenceLookup.Count; } }

	public object SyncRoot = new();

	// Even though it may seem like it, consider that the SerializableReferenceHandler isn't really a hot path anyways, except during serialization and deserialization.
	private Dictionary<SerializableReference, ISerializableReference> serializableReferenceLookup = new(); // Possible optimization: would it be faster to have separate dictionaries for string lookups and guid lookups, or would it work out to about the same?

	private Dictionary<SerializableReference, List<Action<ISerializableReference>>> eventualReferencesToFulfill = new();

	public bool AllowEventualReferences { get; private set; }

	public bool AllowUnresolvedEventualReferences { get; private set; }

	public void EnterAllowEventualReferenceMode(bool allowUnresolvedEventualReferences)
	{
		lock (SyncRoot)
		{
			AllowEventualReferences = true;
			AllowUnresolvedEventualReferences = allowUnresolvedEventualReferences;
		}
	}

	public void ExitAllowEventualReferenceMode()
	{
		lock (SyncRoot)
		{
			AllowEventualReferences = false;

			if (!AllowUnresolvedEventualReferences)
			{
				if (eventualReferencesToFulfill.Count > 0)
				{
					throw new Exception("Not all eventual references have been resolved yet! Either this was called too early, or there is a missing reference.");
				}
			}

			AllowUnresolvedEventualReferences = false;
		}
	}

	public bool ContainsReference(ISerializableReference serializableReference)
	{
		lock (SyncRoot)
		{
			return serializableReferenceLookup.ContainsKey(serializableReference.SerializableReference);
		}
	}

	public bool ContainsReference(Guid guid)
	{
		lock (SyncRoot)
		{
			return serializableReferenceLookup.ContainsKey(guid);
		}
	}

	public bool ContainsReference(string name)
	{
		lock (SyncRoot)
		{
			return serializableReferenceLookup.ContainsKey(name);
		}
	}

	public bool ContainsReference(SerializableReference serializableReference)
	{
		lock (SyncRoot)
		{
			return serializableReferenceLookup.ContainsKey(serializableReference);
		}
	}

	public void RegisterReference(ISerializableReference serializableReference)
	{
		lock (SyncRoot)
		{
			Debug.Assert(serializableReference.SerializableReference.IsValid, "The SerializableReference that was attempted to be registered was invalid.");

			if (AllowEventualReferences)
			{
				if (eventualReferencesToFulfill.ContainsKey(serializableReference.SerializableReference))
				{
					foreach (var eventualReferenceCallback in eventualReferencesToFulfill[serializableReference.SerializableReference])
					{
						eventualReferenceCallback(serializableReference);
					}

					eventualReferencesToFulfill.Remove(serializableReference.SerializableReference);
				}
			}

			serializableReferenceLookup.Add(serializableReference.SerializableReference, serializableReference);
		}
	}

	public void GetEventualReference(SerializableReference serializableReference, Action<ISerializableReference> referenceRegisteredCallback)
	{
		lock (SyncRoot)
		{
			if (ContainsReference(serializableReference))
			{
				referenceRegisteredCallback(this[serializableReference]);
			}
			else
			{
				if (!AllowEventualReferences)
				{
					throw new InvalidOperationException($"Cannot use eventual references when {nameof(AllowEventualReferences)} mode is not active!");
				}

				if (eventualReferencesToFulfill.ContainsKey(serializableReference))
				{
					eventualReferencesToFulfill[serializableReference].Add(referenceRegisteredCallback);
				}
				else
				{
					eventualReferencesToFulfill.TryAdd(serializableReference, new List<Action<ISerializableReference>>() { referenceRegisteredCallback });
				}
			}
		}
	}

	public void RemoveReference(ISerializableReference serializableReference)
	{
		lock (SyncRoot)
		{
			serializableReferenceLookup.Remove(serializableReference.SerializableReference);
		}
	}

	public void Clear()
	{
		lock (SyncRoot)
		{
			ExitAllowEventualReferenceMode();

			serializableReferenceLookup.Clear();
		}
	}


	public void Serialize(XmlWriter writer)
	{
		lock (SyncRoot)
		{
			if (AllowEventualReferences)
			{
				// add justification why in comment here
				throw new InvalidOperationException($"Cannot serialize when {nameof(AllowEventualReferences)} mode is active!");
			}

			writer.Serialize(serializableReferenceLookup.Values, (w, s) => w.SerializeWithEmbeddedType(s), nameof(SerializationReferenceHandler));
		}
	}

	public static SerializationReferenceHandler Deserialize(XmlReader reader, SerializationData serializationData, bool exitAllowEventualReferenceModeBeforeReturning = true)
	{
		SerializationReferenceHandler referenceHandler = new();

		referenceHandler.EnterAllowEventualReferenceMode(false);

		reader.DeserializeCollection((r) => referenceHandler.RegisterReference((ISerializableReference)r.DeserializeEntitySerializableWithEmbeddedType(serializationData, referenceHandler)), nameof(SerializationReferenceHandler));

		if (exitAllowEventualReferenceModeBeforeReturning)
		{
			referenceHandler.ExitAllowEventualReferenceMode();
		}

		return referenceHandler;
	}

	public void DeserializeInto(XmlReader reader, SerializationData serializationData, bool exitAllowEventualReferenceModeBeforeReturning = true)
	{
		lock (SyncRoot)
		{
			EnterAllowEventualReferenceMode(false);

			reader.DeserializeCollection((r) => RegisterReference((ISerializableReference)r.DeserializeEntitySerializableWithEmbeddedType(serializationData, this)), nameof(SerializationReferenceHandler));

			if (exitAllowEventualReferenceModeBeforeReturning)
			{
				ExitAllowEventualReferenceMode();
			}
		}
	}
}
