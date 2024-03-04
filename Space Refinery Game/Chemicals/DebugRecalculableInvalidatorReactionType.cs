using Space_Refinery_Engine;

namespace Space_Refinery_Game;

public sealed class DebugRecalculableInvalidatorReactionType : ReactionType
{
	public override string Reaction => "N/A";

	public override void Tick(IntervalUnit _, ResourceContainer resourceContainer, ILookup<Type, ReactionFactor> _2, ICollection<ReactionFactor> _3)
	{
		if (MainGame.DebugSettings.AccessSetting<BooleanDebugSetting>("Always invalidate all recalculables"))
		{
			resourceContainer.InvalidateRecalculables();
		}
	}
}
