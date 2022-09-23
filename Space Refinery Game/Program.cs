using FixedPrecision;
using FXRenderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.StartupUtilities;
using Veldrid.Utilities;

namespace Space_Refinery_Game;

public static class Program
{
	public static void Main()
	{
		System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

		Window window = new("Space Refinery");

		GraphicsDeviceOptions options = new GraphicsDeviceOptions(
			debug: false,
			swapchainDepthFormat: PixelFormat.R16_UNorm,
			syncToVerticalBlank: false,
			resourceBindingModel: ResourceBindingModel.Improved,
			preferDepthRangeZeroToOne: true,
			preferStandardClipSpaceYDirection: true);
		//#if DEBUG
		options.Debug = true;
		//#endif
		var graphicsDevice = VeldridStartup.CreateGraphicsDevice(window.SdlWindow, options, GraphicsBackend.Direct3D11);
		var factory = new DisposeCollectorResourceFactory(graphicsDevice.ResourceFactory);

		window.SetUp(graphicsDevice, factory);

		window.CaptureMouse = true;

		MainGame mainGame = new();

		mainGame.Start(window, graphicsDevice, factory, window.CreateSwapchain());
	}
}
