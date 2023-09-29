﻿using Space_Refinery_Game;
﻿using System.Diagnostics;

namespace Space_Refinery_Utilities;

/// <remarks>
/// This class is entirely thread safe.
/// </remarks>
public static class Logging
{
	private static object syncRoot = new();

	public static void Log(string logText)
	{
		Console.WriteLine($"[LOG] {logText}");
	}

	public static void LogSimulation(string logText)
	{
		Console.WriteLine($"[SIMUL@{Time.TicksElapsed}] {logText}");
	}

	/// <remarks>
	/// This method is ignored in builds without the "IncludeAdditionalDebugLogging" conditional compilation symbol is defined.
	/// </remarks>
	[Conditional("IncludeAdditionalDebugLogging")]
	// TODO: make this, and maybe all logging methods, faster by avoiding the slow console. Perhaps use a separate thread for that.
	public static void LogDebug(string logText)
	{
		Console.WriteLine($"[DEBUG] {logText}");
	}

	public static void LogError(string logText)
	{
		LogColor($"[ERROR@{Time.TicksElapsed}={Time.CurrentTickTime} s] {logText}", ConsoleColor.Red);
	}

	public static void LogWarning(string logText)
	{
		LogColor($"[WARN] {logText}", ConsoleColor.Yellow);
	}

	public static void LogColor(string logText, ConsoleColor color)
	{
		lock (syncRoot) // It's okay to lock here, becuase Logging an error doesn't have to be very fast.
		{
			var originalColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.WriteLine(logText);
			Console.ForegroundColor = originalColor;
		}
	}
}
