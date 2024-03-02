using FXRenderer;
using Space_Refinery_Utilities;
using System.Globalization;
using Veldrid;
using Veldrid.StartupUtilities;
using Veldrid.Utilities;

namespace Space_Refinery_Engine;

public static class Program
{
	public static void Main()
	{
		Thread.CurrentThread.Name = "Main";

		Console.ForegroundColor = ConsoleColor.White; // Sometimes while debugging, the program could crash while the logger is printing colored text. This will ensure that when the console is reused, it has the right color from the start!

		Logging.LogLevel logLevel;

#if DEBUG
		logLevel = Logging.LogLevel.Debug;
#else
		logLevel = Logging.LogLevel.Release;
#endif

		Logging.SetUp(logLevel);

		Logging.LogDebug($"Logs from {nameof(Logging.LogDebug)} are included in this build.");
		Logging.LogLegend($"Build version: {"haven't started with this yet"}");

		CultureInfo.CurrentCulture = (System.Globalization.CultureInfo)CultureInfo.InvariantCulture.Clone();
		CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator = " ";
		CultureInfo.CurrentCulture.NumberFormat.NumberGroupSizes = CultureInfo.GetCultureInfo("se-SE").NumberFormat.NumberGroupSizes;
		//System.Globalization.CultureInfo.CurrentCulture.NumberFormat = DecimalNumber.NumberFormat;
		// The previous line cannot be uncommented because the number format is not complete.

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
