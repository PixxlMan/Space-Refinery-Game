﻿using System.Collections.Concurrent;
using System.Xml;

namespace Space_Refinery_Engine
{
	public abstract class ReactionType : ISerializableReference
	{
		public ReactionType()
		{
			ReactionTypes.Add(this);
		}

		public static ICollection<ReactionType> GetAllPossibleReactionTypes(HashSet<ChemicalType> availableChemicals)
		{
			// If there are no chemicals available there is no point in trying to find possible reactions or adding the universal reactions.
			// This is an optimization which avoids unnecessary work in the rest of the method, but also prevents unnecessary work ticking the universal reactions.
			if (availableChemicals.Count == 0)
			{
				return Array.Empty<ReactionType>();
			}

			HashSet<ReactionType> initialPossibleReactions = new(); // Find all reaction types that share a necessary chemical with what is available.

			foreach (var chemical in availableChemicals)
			{
				if (PossibleReactionsPerChemicalType.ContainsKey(chemical))
				{
					initialPossibleReactions.UnionWith(PossibleReactionsPerChemicalType[chemical]);
				}
			}

			HashSet<ReactionType> refinedPossibleReactions = new(); // Eliminate all reaction types whoose necessary chemicals are not fully satisfied.

			foreach (var possibleReaction in initialPossibleReactions)
			{
				if (possibleReaction.NecessaryChemicals.IsSubsetOf(availableChemicals))
				{
					refinedPossibleReactions.Add(possibleReaction);
				}
			}

			foreach (var universalReaction in UniversalReactions) // Make sure to include universal reactions.
			{
				refinedPossibleReactions.Add(universalReaction);
			}

			return refinedPossibleReactions;
		}

		public static List<ReactionType> ReactionTypes = new();

		public static ConcurrentDictionary<ChemicalType, HashSet<ReactionType>> PossibleReactionsPerChemicalType = new();

		public static ConcurrentBag<ReactionType> UniversalReactions = new();

		public string ReactionName { get; protected set; }

		public abstract string Reaction { get; }

		public HashSet<ChemicalType> NecessaryChemicals { get; private set; }

		public SerializableReference SerializableReference { get; private set; }

		public bool CanOccurSpontaneously { get; protected set; }

		public abstract void Tick(IntervalUnit tickInterval, ResourceContainer resourceContainer, ILookup<Type, ReactionFactor> reactionFactors, ICollection<ReactionFactor> producedReactionFactors);

		public virtual void SerializeState(XmlWriter writer)
		{
			writer.WriteStartElement(nameof(ReactionType));
			{
				writer.SerializeReference(this);

				writer.Serialize(ReactionName, nameof(ReactionName));

				writer.Serialize(CanOccurSpontaneously, nameof(CanOccurSpontaneously));

				writer.Serialize(NecessaryChemicals, nameof(NecessaryChemicals));
			}
			writer.WriteEndElement();
		}

		public virtual void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
		{
			reader.ReadStartElement(nameof(ReactionType));
			{
				SerializableReference = reader.ReadReference();

				ReactionName = reader.ReadString(nameof(ReactionName));

				CanOccurSpontaneously = reader.DeserializeBoolean(nameof(CanOccurSpontaneously));

				ConcurrentBag<ChemicalType> necessaryChemicalTypes = new();
				reader.DeserializeReferenceCollection(necessaryChemicalTypes, referenceHandler, nameof(NecessaryChemicals));

				serializationData.DeserializationCompleteEvent += () =>
				{
					NecessaryChemicals = necessaryChemicalTypes.ToHashSet();

					if (CanOccurSpontaneously)
					{
						if (NecessaryChemicals.Count != 0)
						{
							foreach (ChemicalType chemicalType in NecessaryChemicals)
							{
								if (PossibleReactionsPerChemicalType.ContainsKey(chemicalType))
								{
									PossibleReactionsPerChemicalType[chemicalType].Add(this);
								}
								else
								{
									PossibleReactionsPerChemicalType.TryAdd(chemicalType, new() { this });
								}
							}
						}
						else // No necessary chemicals means it's a universal reaction.
						{
							UniversalReactions.Add(this);
						}
					}
				};
			}
			reader.ReadEndElement();
		}
	}
}
