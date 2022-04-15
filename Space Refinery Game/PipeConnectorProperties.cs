using FixedPrecision;

namespace Space_Refinery_Game
{
	[Serializable]
	public struct PipeConnectorProperties
	{
		public PipeShape Shape;

		public FixedDecimalInt4 ConnectorDiameter;

		public FixedDecimalInt4 ConnectorFlowAreaDiameter;
	}
}