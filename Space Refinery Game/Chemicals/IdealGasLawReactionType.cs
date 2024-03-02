using Space_Refinery_Engine;

namespace Space_Refinery_Game;

public sealed class IdealGasLawReactionType : ReactionType
{
	public override string Reaction => "P * V = n * k * T";


	public override void Tick(IntervalUnit _, ResourceContainer resourceContainer, ILookup<Type, ReactionFactor> _2, ICollection<ReactionFactor> _3)
	{
		lock (resourceContainer.SyncRoot)
		{
			resourceContainer.InvalidateRecalculables();
		}
	}
}
