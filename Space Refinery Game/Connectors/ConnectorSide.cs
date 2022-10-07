namespace Space_Refinery_Game
{
	public enum ConnectorSide
	{
		A,
		B
	}

	public static class ConnectorSideExtensions
	{
		public static ConnectorSide Opposite(this ConnectorSide connector)
		{
			switch (connector)
			{
				case ConnectorSide.A:
					return ConnectorSide.B;
				case ConnectorSide.B:
					return ConnectorSide.A;
				default:
					throw new ArgumentException("Invalid ConnectorSide enum.", nameof(connector));
			}
		}
	}
}