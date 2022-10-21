using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Space_Refinery_Game
{
	public abstract class ReactionType : ISerializableReference
	{
		public ReactionType()
		{
			ReactionTypes.Add(this);
		}

		public static List<ReactionType> ReactionTypes = new();

		public string ReactionName { get; protected set; }

		public ICollection<ChemicalType> NecessaryChemicals { get; private set; }

		public Guid SerializableReferenceGUID { get; private set; }

		public abstract void Tick(DecimalNumber interval, ResourceContainer resourceContainer);

		public virtual void SerializeState(XmlWriter writer)
		{
			writer.WriteStartElement(nameof(ReactionType));
			{			
				writer.SerializeReference(this);

				writer.Serialize(ReactionName, nameof(ReactionName));

				writer.Serialize(NecessaryChemicals, nameof(NecessaryChemicals));
			}
			writer.WriteEndElement();
		}

		public virtual void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			reader.ReadStartElement(nameof(ReactionType));
			{
				SerializableReferenceGUID = reader.ReadReferenceGUID();

				ReactionName = reader.ReadString(nameof(ReactionName));

				ConcurrentBag<ChemicalType> necessaryChemicalTypes = new();
				reader.DeserializeReferenceCollection(necessaryChemicalTypes, referenceHandler, nameof(NecessaryChemicals));

				serializationData.SerializationCompleteEvent += () =>
				{
					NecessaryChemicals = necessaryChemicalTypes.ToArray();
				};
			}
			reader.ReadEndElement();
		}
	}
}
