namespace Space_Refinery_Utilities;

public static class Calculations
{
	/// <summary>
	/// [J/K⁻¹] k = 8.314 J/K⁻¹
	/// </summary>
	public static readonly DecimalNumber GasConstant = (DecimalNumber)8.314;

	public static PressureUnit PressureIdealGasLaw(MolesUnit gasSubstanceAmount, TemperatureUnit averageTemperature, VolumeUnit volume)
	{
		// P * V = n * k * T
		// P = pressure [kg/m³]
		// V = volume [m³]
		// n = substance amount [mol]
		// k = gas constant (8.314 J/K⁻¹) [J/K⁻¹]
		// T = temperature [K]
		//
		// Solve for P:
		// P = (n * k * T) / V

		var gasMolesDecNum = (DecimalNumber)gasSubstanceAmount;
		var avgTempDecNum = (DecimalNumber)averageTemperature;
		var volDecNum = (DecimalNumber)volume;

		PressureUnit pressure = new((gasMolesDecNum * GasConstant * avgTempDecNum) / (volDecNum));

		return pressure;
	}

	public static TemperatureUnit TemperatureIdealGasLaw(MolesUnit gasSubstanceAmount, PressureUnit pressure, VolumeUnit volume)
	{
		// P * V = n * k * T
		// P = pressure [kg/m³]
		// V = volume [m³]
		// n = substance amount [mol]
		// k = gas constant (8.314 J/K⁻¹) [J/K⁻¹]
		// T = temperature [K]
		//
		// Solve for T:
		// T = (P * V) / (n * k)

		var gasMolesDecNum = (DecimalNumber)gasSubstanceAmount;
		var pressureDecNum = (DecimalNumber)pressure;
		var volDecNum = (DecimalNumber)volume;

		TemperatureUnit temperature = new((pressureDecNum * volDecNum) / (gasMolesDecNum * GasConstant));

		return temperature;
	}

	public static void IdealGasLawSolveRungeKutta(MolesUnit gasSubstanceAmount, TemperatureUnit initialTemperature, PressureUnit initialPressure, VolumeUnit volume, DecimalNumber stepSize, int iterations, out TemperatureUnit temperature, out PressureUnit pressure)
	{
		temperature = initialTemperature;
		pressure = initialPressure;

		if (DN.Difference((DN)TemperatureIdealGasLaw(gasSubstanceAmount, initialPressure, volume), (DN)TemperatureIdealGasLaw(gasSubstanceAmount, PressureIdealGasLaw(gasSubstanceAmount, initialTemperature, volume), volume)) < 0.1)
		{
			return;
		}

		for (int i = 0; i < iterations; i++)
		{
			TemperatureUnit oldTemperature = temperature;
			PressureUnit oldPressure = pressure;

			// Update temperature based on pressure
			temperature = (TemperatureUnit)RungeKuttaStep((P, T) => ((DN)gasSubstanceAmount * GasConstant * T - P * (DN)volume) / ((DN)volume * (DN)gasSubstanceAmount * GasConstant), (DN)pressure, (DN)temperature, stepSize);

			// Update pressure based on temperature
			pressure = (PressureUnit)RungeKuttaStep((T, P) => (P * (DN)volume - (DN)gasSubstanceAmount * GasConstant * T) / ((DN)gasSubstanceAmount * GasConstant * T), (DN)temperature, (DN)pressure, stepSize);

			i++;
		}
	}

	/// <summary>
	/// -273.15 ⁰C = absolute zero = 0 K
	/// </summary>
	public static readonly DecimalNumber AbsoluteZeroInCelcius = -273.15;

	public static DecimalNumber TemperatureToCelcius(TemperatureUnit temperature)
	{
		return (DecimalNumber)temperature + AbsoluteZeroInCelcius;
	}

	public static TemperatureUnit CelciusToTemperature(DecimalNumber celcius)
	{
		return (TemperatureUnit)(celcius - AbsoluteZeroInCelcius);
	}

	// The code is not from here, however it's a good explaination: https://www.geeksforgeeks.org/runge-kutta-4th-order-method-solve-differential-equation/
	public static DecimalNumber RungeKuttaStep(Func<DecimalNumber, DecimalNumber, DecimalNumber> f, DecimalNumber x0, DecimalNumber y0, DecimalNumber h)
	{
		DecimalNumber k1 = h * f(x0, y0);
		DecimalNumber k2 = h * f(x0 + 0.5 * h, y0 + 0.5 * k1);
		DecimalNumber k3 = h * f(x0 + 0.5 * h, y0 + 0.5 * k2);
		DecimalNumber k4 = h * f(x0 + h, y0 + k3);
		return y0 + (1.0 / 6.0) * (k1 + 2 * k2 + 2 * k3 + k4);
	}
}
