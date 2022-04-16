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
		FixedDecimalInt4 fix = 1;

		var chems = ChemicalType.LoadChemicalTypes(Path.Combine(Environment.CurrentDirectory, "Assets", "Chemical types"));

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
