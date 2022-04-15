using FixedPrecision;

namespace Space_Refinery_Game
{
	[Serializable]
	public struct PipeProperties
	{
		public PipeShape Shape;

		public FixedDecimalInt4 WallInsulation;

		public FixedDecimalInt4 FlowableRadius;

		public FixedDecimalInt4 FlowableLength;

		public FixedDecimalInt4 FlowableVolume;
	}
}