using FixedPrecision;
using FXRenderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Veldrid;

namespace Space_Refinery_Game;

public static class Program
{
	public static void Main()
	{
		var chem = new ChemicalType()
		{
			ChemicalName = "H20",
		};

		chem.PlasmaPhaseType = new PlasmaType(chem, "Water Plasma", (FixedDecimalInt4).1);
		chem.GasPhaseType = new GasType(chem, "Water Vapor", (FixedDecimalInt4).2);
		chem.LiquidPhaseType = new LiquidType(chem, "Water", (FixedDecimalInt4)1);
		chem.SolidPhaseType = new SolidType(chem, "Ice", (FixedDecimalInt4).7);

		var stream = File.OpenWrite(@"R:\H20Chem");

		JsonSerializer.Serialize(stream, chem, new JsonSerializerOptions() { IncludeFields = true, WriteIndented = true });

		stream.Close();

		var stream2 = File.OpenRead(@"R:\H20Chem");

		var chem2 = JsonSerializer.Deserialize<ChemicalType>(stream2, new JsonSerializerOptions() { IncludeFields = true });

		Environment.Exit(1337);

		System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

		Window window = new("Space Refinery");

		window.CaptureMouse = true;

		MainGame mainGame = new();

		window.GraphicsDeviceCreated += delegate (GraphicsDevice gd, ResourceFactory factory, Swapchain swapchain)
		{
			mainGame.Start(window, gd, factory, swapchain);
		};

		window.Run();
	}
}
