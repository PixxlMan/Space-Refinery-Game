namespace Space_Refinery_Game
{
	public struct ElectricalCurrent : ReactionFactor
	{
		public EnergyUnit ElectricalEnergy;

		public ElectricalCurrent(EnergyUnit electricalEnergy)
		{
			ElectricalEnergy = electricalEnergy;
		}
	}
}
