namespace Space_Refinery_Game
{
	public struct ElectricalCurrent : ReactionFactor
	{
		public DecimalNumber ElectricalEnergy;

		public ElectricalCurrent(DecimalNumber electricalEnergy)
		{
			ElectricalEnergy = electricalEnergy;
		}
	}
}
