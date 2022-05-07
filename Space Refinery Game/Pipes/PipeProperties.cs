using FixedPrecision;

namespace Space_Refinery_Game
{
	[Serializable]
	public struct PipeProperties
	{
		public PipeShape Shape;

		public FixedDecimalLong8 WallInsulation;

		public FixedDecimalLong8 FlowableRadius;

		public FixedDecimalLong8 FlowableLength;

		public FixedDecimalLong8 FlowableVolume;

		public FixedDecimalLong8 Friction;
	}
}