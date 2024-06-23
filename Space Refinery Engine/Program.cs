using Space_Refinery_Game_Renderer;
using System.Globalization;
using System.Reflection;
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
		Logging.LogLegend($"Commit hash: {Assembly.GetExecutingAssembly()!.GetCustomAttributes<AssemblyMetadataAttribute>().Where((a) => a.Key == "SourceRevisionId").Single().Value}");

		CultureInfo.CurrentCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
		CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator = " ";
		CultureInfo.CurrentCulture.NumberFormat.NumberGroupSizes = CultureInfo.GetCultureInfo("se-SE").NumberFormat.NumberGroupSizes;
		//System.Globalization.CultureInfo.CurrentCulture.NumberFormat = DecimalNumber.NumberFormat;
		// The previous line cannot be uncommented because the number format is not complete.

		Window window = new("Loading...");

		GraphicsDeviceOptions options = new(
#if DEBUG
			debug: true,
#else
			debug: false,
#endif
			swapchainDepthFormat: PixelFormat.R16_UNorm,
			syncToVerticalBlank: false,
			resourceBindingModel: ResourceBindingModel.Improved,
			preferDepthRangeZeroToOne: true,
			preferStandardClipSpaceYDirection: true);
		var graphicsDevice = VeldridStartup.CreateGraphicsDevice(window.SdlWindow, options, GraphicsBackend.Direct3D11);
		var factory = new DisposeCollectorResourceFactory(graphicsDevice.ResourceFactory);

		window.SetUp(graphicsDevice, factory);
		window.CaptureMouse = true;
		var swapchain = window.CreateSwapchain();

		Initialization initialization = new();
		initialization.Start(window, graphicsDevice, factory, swapchain);
	}
}
