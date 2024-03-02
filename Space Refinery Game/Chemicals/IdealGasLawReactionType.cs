using Space_Refinery_Engine;

namespace Space_Refinery_Game;

public sealed class IdealGasLawReactionType : ReactionType
{
	public override string Reaction => "";

	// resourceUnitsToAdd is declared as a field in order to avoid having to recreate it each tick. It is always cleared after use.
	private List<ResourceUnitData> resourceUnitsToAdd = new();

	public override void Tick(IntervalUnit _, ResourceContainer resourceContainer, ILookup<Type, ReactionFactor> _2, ICollection<ReactionFactor> _3)
	{
		lock (resourceContainer.SyncRoot)
		{
			resourceContainer.InvalidateRecalcuables();
		}

		resourceUnitsToAdd.Clear();
	}
}
