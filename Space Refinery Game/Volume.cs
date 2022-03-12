using FixedPrecision;

namespace Space_Refinery_Game
{
	public class Volume
	{
		private Dictionary<IFluid, FixedDecimalInt4> fluidConcentration = new();

		private FixedDecimalInt4 totalVolume;

		private Volume(Dictionary<IFluid, FixedDecimalInt4> fluidConcentration, FixedDecimalInt4 totalVolume)
		{
			this.fluidConcentration = fluidConcentration;
			this.totalVolume = totalVolume;
		}

		public FixedDecimalInt4 GetDensity()
		{
			FixedDecimalInt4 totalDensity = 0;

			foreach (var fluid in fluidConcentration.Keys)
			{
				totalDensity += fluid.InstanceDensity;
			}

			return totalDensity / fluidConcentration.Keys.Count;
		}

		public void AddToVolume(Volume volume)
		{
			foreach (var keyValuePair in volume.fluidConcentration)
			{
				AddToVolume(keyValuePair.Key, keyValuePair.Value);
			}
		}

		public void AddToVolume(IFluid fluid, FixedDecimalInt4 volume)
		{
			if (fluidConcentration.ContainsKey(fluid))
			{
				fluidConcentration[fluid] += volume;
				totalVolume += volume;
			}
			else
			{
				fluidConcentration.Add(fluid, volume);
				totalVolume += volume;
			}
		}

		public Volume TakePart(FixedDecimalInt4 volume)
		{
			FixedDecimalInt4 ratio = volume / totalVolume;

			Dictionary<IFluid, FixedDecimalInt4> partFluidConcentration = new();

			FixedDecimalInt4 totalFluid = 0;

			foreach (var keyValuePair in fluidConcentration)
			{
				partFluidConcentration.Add(
					keyValuePair.Key,
					keyValuePair.Value * ratio
					);

				totalFluid += keyValuePair.Value * ratio;
			}

			return new(partFluidConcentration, totalFluid);
		}
	}
}