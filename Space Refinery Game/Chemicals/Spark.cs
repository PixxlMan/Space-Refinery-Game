namespace Space_Refinery_Game
{
	public struct Spark : ReactionFactor
	{
		/// <summary>
		/// [J]
		/// </summary>
		public DecimalNumber SparkEnergy;

		public Spark(DecimalNumber sparkEnergy)
		{
			SparkEnergy = sparkEnergy;
		}
	}
}